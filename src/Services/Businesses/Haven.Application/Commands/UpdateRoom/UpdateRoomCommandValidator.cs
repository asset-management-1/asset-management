namespace Haven.Application.Commands.UpdateRoom;

/// <summary>
/// Validates room update requests.
/// </summary>
public class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
{
    /// <summary>
    /// Creates validation rules for room updates.
    /// </summary>
    public UpdateRoomCommandValidator()
    {
        // Room id is required; every other room field is optional and keeps current value when omitted.
        RuleFor(x => x.Id).RequiredGuid();
        RuleFor(x => x.FloorNumber).GreaterThanZeroWhenPresent();
        RuleFor(x => x.Name).MaxLen(255);
        RuleFor(x => x.TypeCode).MaxLen(100);
        RuleFor(x => x.RentalModeCode).MaxLen(100);
        RuleFor(x => x.AreaSqm).GreaterThanZeroWhenPresent();
        RuleFor(x => x.BaseRentAmount).GreaterOrEqualZeroWhenPresent();
        RuleFor(x => x.DefaultDepositAmount).GreaterOrEqualZeroWhenPresent();
        RuleFor(x => x.TotalBeds).GreaterThanZeroWhenPresent();

        // Switching a room into shared-bed mode requires a capacity in the same request.
        RuleFor(x => x.TotalBeds)
            .Required()
            .When(x => string.Equals(
                x.RentalModeCode,
                MASTER_CODE_RENTAL_MODE_SHARED_BED,
                StringComparison.OrdinalIgnoreCase));
        RuleFor(x => x.TotalBeds)
            .Null()
            .When(x => !string.IsNullOrWhiteSpace(x.RentalModeCode)
                       && !string.Equals(
                           x.RentalModeCode,
                           MASTER_CODE_RENTAL_MODE_SHARED_BED,
                           StringComparison.OrdinalIgnoreCase));

        // Override modes are optional section instructions; absent mode means no change for that section.
        RuleFor(x => x.ChargePolicyMode)
            .Must(mode => string.Equals(mode, ROOM_OVERRIDE_MODE_COMMON, StringComparison.OrdinalIgnoreCase)
                          || string.Equals(mode, ROOM_OVERRIDE_MODE_CUSTOM, StringComparison.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrWhiteSpace(x.ChargePolicyMode))
            .WithMessage(ApplicationErrorConstants.RoomErrors.ERROR_ROOM_OVERRIDE_MODE_INVALID);

        RuleFor(x => x.PackageMode)
            .Must(mode => string.Equals(mode, ROOM_OVERRIDE_MODE_COMMON, StringComparison.OrdinalIgnoreCase)
                          || string.Equals(mode, ROOM_OVERRIDE_MODE_CUSTOM, StringComparison.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrWhiteSpace(x.PackageMode))
            .WithMessage(ApplicationErrorConstants.RoomErrors.ERROR_ROOM_OVERRIDE_MODE_INVALID);

        // Room-level charge policy rows are bounded before custom override sync applies them.
        RuleFor(x => x.ChargePolicies).MaxCount(MAX_CHARGE_POLICIES);

        RuleForEach(x => x.ChargePolicies).SetValidator(new UpdateRoomChargePolicyRequestDtoValidator());

        // Room-level packages are bounded before custom override sync applies them.
        RuleFor(x => x.Packages).MaxCount(MAX_PACKAGE_TEMPLATES);

        RuleForEach(x => x.Packages).SetValidator(new UpdateRoomPackageRequestDtoValidator());

        // Policy payloads require a mode so the backend knows whether to clear or replace overrides.
        RuleFor(x => x.ChargePolicies)
            .Empty()
            .When(x => string.IsNullOrWhiteSpace(x.ChargePolicyMode));

        // Package payloads require a mode so the backend knows whether to clear or replace overrides.
        RuleFor(x => x.Packages)
            .Empty()
            .When(x => string.IsNullOrWhiteSpace(x.PackageMode));

        // Common mode means delete room-level charge overrides and fall back to property setup.
        RuleFor(x => x.ChargePolicies)
            .Empty()
            .When(x => string.Equals(x.ChargePolicyMode, ROOM_OVERRIDE_MODE_COMMON, StringComparison.OrdinalIgnoreCase));

        // Custom mode means the room must supply charge-policy rows to add or update.
        RuleFor(x => x.ChargePolicies)
            .NotEmptyCollection()
            .When(x => string.Equals(x.ChargePolicyMode, ROOM_OVERRIDE_MODE_CUSTOM, StringComparison.OrdinalIgnoreCase))
            .WithMessage(ApplicationErrorConstants.RoomErrors.ERROR_ROOM_CUSTOM_CHARGE_POLICIES_REQUIRED);

        // Common mode means delete room-level package overrides and fall back to property setup.
        RuleFor(x => x.Packages)
            .Empty()
            .When(x => string.Equals(x.PackageMode, ROOM_OVERRIDE_MODE_COMMON, StringComparison.OrdinalIgnoreCase));

        // Custom mode means the room must supply package rows to add or update.
        RuleFor(x => x.Packages)
            .NotEmptyCollection()
            .When(x => string.Equals(x.PackageMode, ROOM_OVERRIDE_MODE_CUSTOM, StringComparison.OrdinalIgnoreCase))
            .WithMessage(ApplicationErrorConstants.RoomErrors.ERROR_ROOM_CUSTOM_PACKAGES_REQUIRED);
    }
}

/// <summary>
/// Validates one room charge policy payload.
/// </summary>
public class UpdateRoomChargePolicyRequestDtoValidator : AbstractValidator<UpdateRoomChargePolicyRequestDto>
{
    /// <summary>
    /// Creates validation rules for a room charge policy.
    /// </summary>
    public UpdateRoomChargePolicyRequestDtoValidator()
    {
        // Existing policies are partial rows; new policies must carry every field needed for persistence.
        RuleFor(x => x.Id).NotDefaultWhenPresent();
        When(x => !x.Id.HasValue, () =>
        {
            RuleFor(x => x.Code).Required();
            RuleFor(x => x.Name).Required();
            RuleFor(x => x.Amount).Required();
            RuleFor(x => x.CalculationMethodCode).Required();
        });

        RuleFor(x => x.Code).MaxLen(100);
        RuleFor(x => x.Name).MaxLen(255);
        RuleFor(x => x.Amount).GreaterOrEqualZeroWhenPresent();
        RuleFor(x => x.CalculationMethodCode).MaxLen(100);

        // Vehicle-type changes include the policy code so parking semantics remain deterministic.
        RuleFor(x => x.Code)
            .Required()
            .When(x => !string.IsNullOrWhiteSpace(x.VehicleTypeCode));

        // Parking policies must identify the vehicle type they price; other policies must stay generic.
        RuleFor(x => x.VehicleTypeCode)
            .Required()
            .MaxLen(100)
            .When(x => string.Equals(x.Code, MASTER_CODE_INVOICE_LINE_TYPE_PARKING, StringComparison.OrdinalIgnoreCase));
        RuleFor(x => x.VehicleTypeCode)
            .Null()
            .When(x => !string.IsNullOrWhiteSpace(x.Code)
                       && !string.Equals(x.Code, MASTER_CODE_INVOICE_LINE_TYPE_PARKING, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Validates one room package template payload.
/// </summary>
public class UpdateRoomPackageRequestDtoValidator : AbstractValidator<UpdateRoomPackageRequestDto>
{
    /// <summary>
    /// Creates validation rules for a room package template.
    /// </summary>
    public UpdateRoomPackageRequestDtoValidator()
    {
        // Existing room-level packages are matched by public id; new custom packages omit it.
        RuleFor(x => x.Id).NotDefaultWhenPresent();

        // Existing packages merge submitted fields; new packages require display and price values.
        When(x => !x.Id.HasValue, () =>
        {
            RuleFor(x => x.Name).Required();
            RuleFor(x => x.PriceAdjustment).Required();
        });

        RuleFor(x => x.Name).MaxLen(255);
        RuleFor(x => x.PriceAdjustment).GreaterOrEqualZeroWhenPresent();

        // Package item count is bounded because each item becomes one persisted package-item row.
        RuleFor(x => x.Items).MaxCount(MAX_PACKAGE_ITEMS);

        RuleForEach(x => x.Items).SetValidator(new UpdateRoomPackageItemRequestDtoValidator());
    }
}

/// <summary>
/// Validates one room package item payload.
/// </summary>
public class UpdateRoomPackageItemRequestDtoValidator : AbstractValidator<UpdateRoomPackageItemRequestDto>
{
    /// <summary>
    /// Creates validation rules for a package item.
    /// </summary>
    public UpdateRoomPackageItemRequestDtoValidator()
    {
        // Package items are simple labels such as furniture or included service names.
        RuleFor(x => x.Name).Required().MaxLen(255);
    }
}
