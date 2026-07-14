namespace Haven.Application.Commands.UpdateProperty;

/// <summary>
/// Validates the property update request.
/// </summary>
public class UpdatePropertyCommandValidator : AbstractValidator<UpdatePropertyCommand>
{
    /// <summary>
    /// Creates validation rules for property update.
    /// </summary>
    public UpdatePropertyCommandValidator()
    {
        // Update is body-scoped and should not run without a frontend-safe property identifier.
        RuleFor(x => x.Id).RequiredGuid();

        // Basic editable property fields are optional; omitted fields keep the current value.
        RuleFor(x => x.Name).MaxLen(255);
        RuleFor(x => x.PropertyTypeCode).MaxLen(100);
        RuleFor(x => x.ProvinceCode).MaxLen(30);
        RuleFor(x => x.DistrictCode).MaxLen(30);
        RuleFor(x => x.WardCode).MaxLen(30);
        RuleFor(x => x.StreetAddress).MaxLen(1000);
        RuleFor(x => x.FormattedAddress).MaxLen(1000);
        RuleFor(x => x.Latitude).BetweenInclusiveWhenPresent(-90, 90);
        RuleFor(x => x.Longitude).BetweenInclusiveWhenPresent(-180, 180);

        // When structure is submitted, FE sends only rooms it wants to add or update.
        RuleFor(x => x)
            .Must(StayWithinUpdateRoomLimit)
            .WithMessage(string.Format(
                CultureInfo.InvariantCulture,
                ApplicationErrorConstants.PropertyErrors.ERROR_UPDATE_ROOMS_LIMIT,
                MAX_PROPERTY_STRUCTURE_ROOMS))
            .When(x => x.Structure is not null);

        When(x => x.Structure is not null, () =>
        {
            // Omitted rooms remain unchanged; room deletion is handled by the room delete API.
            RuleFor(x => x.Structure.Floors).NotEmptyCollection();

            RuleForEach(x => x.Structure.Floors)
                .SetValidator(new UpdatePropertyFloorRequestDtoValidator());

            // Existing room ids must point to one edited row only.
            RuleFor(x => x)
                .Must(HaveUniqueExistingRoomIds)
                .WithMessage(ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_UPDATE_ROOM_IDS_UNIQUE);
        });

        // When common charge policies are submitted, only supplied rows are added or updated.
        When(x => x.ChargePolicies is not null, () =>
        {
            RuleFor(x => x.ChargePolicies).MaxCount(MAX_CHARGE_POLICIES);

            RuleForEach(x => x.ChargePolicies)
                .SetValidator(new UpdatePropertyChargePolicyRequestDtoValidator());

            // Existing policy ids must be unique so service sync can match one row deterministically.
            RuleFor(x => x)
                .Must(HaveUniqueExistingPolicyIds)
                .WithMessage(ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_UPDATE_POLICY_IDS_UNIQUE);
        });

        // When common packages are submitted, only supplied package templates are added or updated.
        When(x => x.Packages is not null, () =>
        {
            RuleFor(x => x.Packages).MaxCount(MAX_PACKAGE_TEMPLATES);

            RuleForEach(x => x.Packages)
                .SetValidator(new UpdatePropertyPackageRequestDtoValidator());

            // Existing package ids must be unique so service sync can match one row deterministically.
            RuleFor(x => x)
                .Must(HaveUniquePackageIds)
                .WithMessage(ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_UPDATE_PACKAGE_IDS_UNIQUE);
        });
    }

    /// <summary>
    /// Ensures the submitted room operations stay inside the supported room limit.
    /// </summary>
    /// <param name="command">The update command.</param>
    /// <returns><c>true</c> when the supplied room count is within limit.</returns>
    private static bool StayWithinUpdateRoomLimit(UpdatePropertyCommand command)
    {
        // The limit protects a single structure edit from submitting an unexpectedly large room set.
        var roomCount = (command.Structure.Floors ?? []).Sum(floor => (floor.Rooms ?? []).Count);

        return roomCount <= MAX_PROPERTY_STRUCTURE_ROOMS;
    }

    /// <summary>
    /// Checks whether existing room identifiers are unique across the payload.
    /// </summary>
    /// <param name="command">The update command.</param>
    /// <returns><c>true</c> when no duplicated existing room identifier is supplied.</returns>
    private static bool HaveUniqueExistingRoomIds(UpdatePropertyCommand command)
    {
        var ids = (command.Structure?.Floors ?? [])
            .SelectMany(floor => floor.Rooms ?? [])
            .Where(room => room.Id.HasValue)
            .Select(room => room.Id.Value)
            .ToList();

        // Duplicate ids would make add/update sync ambiguous.
        return ids.Count == ids.Distinct().Count();
    }

    /// <summary>
    /// Checks whether existing charge policy identifiers are unique.
    /// </summary>
    /// <param name="command">The update command.</param>
    /// <returns><c>true</c> when no duplicated policy identifier is supplied.</returns>
    private static bool HaveUniqueExistingPolicyIds(UpdatePropertyCommand command)
    {
        var ids = (command.ChargePolicies ?? [])
            .Where(policy => policy.Id.HasValue)
            .Select(policy => policy.Id.Value)
            .ToList();

        // Duplicate policy ids would make sync order-dependent.
        return ids.Count == ids.Distinct().Count();
    }

    /// <summary>
    /// Checks whether existing package template identifiers are unique.
    /// </summary>
    /// <param name="command">The update command.</param>
    /// <returns><c>true</c> when supplied package identifiers are unique.</returns>
    private static bool HaveUniquePackageIds(UpdatePropertyCommand command)
    {
        var ids = (command.Packages ?? [])
            .Where(package => package.Id.HasValue)
            .Select(package => package.Id.Value)
            .ToList();

        // Duplicate package ids would make sync order-dependent.
        return ids.Count == ids.Distinct().Count();
    }
}

/// <summary>
/// Validates one floor in a property update request.
/// </summary>
public class UpdatePropertyFloorRequestDtoValidator : AbstractValidator<UpdatePropertyFloorRequestDto>
{
    /// <summary>
    /// Creates validation rules for an edited floor.
    /// </summary>
    public UpdatePropertyFloorRequestDtoValidator()
    {
        // Floors remain derived from Units.FloorNumber, so no Floor table identifier is accepted.
        RuleFor(x => x.FloorNumber).BetweenInclusive(1, MAX_PROPERTY_STRUCTURE_FLOORS);

        // Each floor payload is bounded to keep one edit request from becoming too large.
        RuleFor(x => x.Rooms).MaxCount(MAX_PROPERTY_STRUCTURE_ROOMS_PER_FLOOR);

        RuleFor(x => x.Rooms).NotEmptyCollection();
        RuleForEach(x => x.Rooms).SetValidator(new UpdatePropertyRoomRequestDtoValidator());
    }
}

/// <summary>
/// Validates one room in a property update request.
/// </summary>
public class UpdatePropertyRoomRequestDtoValidator : AbstractValidator<UpdatePropertyRoomRequestDto>
{
    /// <summary>
    /// Creates validation rules for an edited room.
    /// </summary>
    public UpdatePropertyRoomRequestDtoValidator()
    {
        // New rooms have no id and must carry enough data to create one complete room row.
        When(x => !x.Id.HasValue, () =>
        {
            RuleFor(x => x.Name).Required().MaxLen(255);
            RuleFor(x => x.TypeCode).Required().MaxLen(100);
            RuleFor(x => x.RentalModeCode).Required().MaxLen(100);
            RuleFor(x => x.IsPetAllowed).NotNull();
        });

        // Existing room ids are optional, but when supplied they must be usable public identifiers.
        When(x => x.Id.HasValue, () =>
        {
            RuleFor(x => x.Id).NotDefaultWhenPresent();
        });

        // Update rows are partial, so editable text and combobox code fields are bounded only when supplied.
        RuleFor(x => x.Name).MaxLen(255);
        RuleFor(x => x.TypeCode).MaxLen(100);
        RuleFor(x => x.RentalModeCode).MaxLen(100);
        RuleFor(x => x.AreaSqm).GreaterThanZeroWhenPresent();
        RuleFor(x => x.BaseRentAmount).GreaterOrEqualZeroWhenPresent();
        RuleFor(x => x.DefaultDepositAmount).GreaterOrEqualZeroWhenPresent();

        // Shared-bed room rows need capacity when creating or explicitly switching the room into shared-bed mode.
        RuleFor(x => x.TotalBeds)
            .Required()
            .GreaterThanZero()
            .When(x => string.Equals(
                x.RentalModeCode,
                MASTER_CODE_RENTAL_MODE_SHARED_BED,
                StringComparison.OrdinalIgnoreCase));

        // Non-shared-bed rows should not carry bed capacity when rental mode is explicitly supplied.
        RuleFor(x => x.TotalBeds)
            .Null()
            .When(x => !string.IsNullOrWhiteSpace(x.RentalModeCode)
                       && !string.Equals(
                           x.RentalModeCode,
                           MASTER_CODE_RENTAL_MODE_SHARED_BED,
                           StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Validates one property-level charge policy in an update request.
/// </summary>
public class UpdatePropertyChargePolicyRequestDtoValidator : AbstractValidator<UpdatePropertyChargePolicyRequestDto>
{
    /// <summary>
    /// Creates validation rules for an edited charge policy.
    /// </summary>
    public UpdatePropertyChargePolicyRequestDtoValidator()
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
        RuleFor(x => x.VehicleTypeCode).MaxLen(100);

        // Vehicle-type changes include the policy code so parking semantics remain deterministic.
        RuleFor(x => x.Code)
            .Required()
            .When(x => !string.IsNullOrWhiteSpace(x.VehicleTypeCode));

        When(
            x => string.Equals(x.Code, MASTER_CODE_INVOICE_LINE_TYPE_PARKING, StringComparison.OrdinalIgnoreCase),
            () =>
        {
            // Parking policy rows must identify the charged vehicle type.
            RuleFor(x => x.VehicleTypeCode).Required().MaxLen(100);
        });

        When(
            x => !string.IsNullOrWhiteSpace(x.Code)
                 && !string.Equals(x.Code, MASTER_CODE_INVOICE_LINE_TYPE_PARKING, StringComparison.OrdinalIgnoreCase),
            () =>
        {
            // Non-parking policy rows should not carry vehicle-only data.
            RuleFor(x => x.VehicleTypeCode).Empty();
        });
    }
}

/// <summary>
/// Validates one package template in a property update request.
/// </summary>
public class UpdatePropertyPackageRequestDtoValidator : AbstractValidator<UpdatePropertyPackageRequestDto>
{
    /// <summary>
    /// Creates validation rules for an edited package template.
    /// </summary>
    public UpdatePropertyPackageRequestDtoValidator()
    {
        // Existing package templates are matched by public id; new package templates omit it.
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

        RuleForEach(x => x.Items).SetValidator(new UpdatePropertyPackageItemRequestDtoValidator());
    }
}

/// <summary>
/// Validates one package item in a property update request.
/// </summary>
public class UpdatePropertyPackageItemRequestDtoValidator : AbstractValidator<UpdatePropertyPackageItemRequestDto>
{
    /// <summary>
    /// Creates validation rules for an edited package item.
    /// </summary>
    public UpdatePropertyPackageItemRequestDtoValidator()
    {
        // Package items are labels such as furniture or included services.
        RuleFor(x => x.Name).Required().MaxLen(255);
    }
}
