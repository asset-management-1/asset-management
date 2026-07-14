namespace Haven.Application.Commands.CreateProperty;

/// <summary>
/// Validates the property creation request.
/// </summary>
public class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    /// <summary>
    /// Creates validation rules for property creation.
    /// </summary>
    public CreatePropertyCommandValidator()
    {
        // Basic property identity and type are required before any DB-backed lookup runs.
        RuleFor(x => x.Name).Required().MaxLen(255);

        RuleFor(x => x.PropertyTypeCode).Required().MaxLen(100);

        // Location fields are optional in V1, but supplied values must stay within expected code/address lengths.
        RuleFor(x => x.ProvinceCode).MaxLen(30);

        RuleFor(x => x.DistrictCode).MaxLen(30);

        RuleFor(x => x.WardCode).MaxLen(30);

        RuleFor(x => x.StreetAddress).MaxLen(1000);

        RuleFor(x => x.FormattedAddress).MaxLen(1000);

        // Coordinates are optional and validated only when the user supplies map data.
        RuleFor(x => x.Latitude).BetweenInclusiveWhenPresent(-90, 90);

        RuleFor(x => x.Longitude).BetweenInclusiveWhenPresent(-180, 180);

        // Structure setup is the final FE-composed floor/room list; backend no longer generates rooms.
        RuleFor(x => x.StructureSetup).Required();

        RuleFor(x => x).Must(StayWithinSubmittedRoomLimit)
            .WithMessage(string.Format(
                CultureInfo.InvariantCulture,
                ApplicationErrorConstants.PropertyErrors.ERROR_GENERATED_UNITS_LIMIT,
                MAX_PROPERTY_STRUCTURE_ROOMS));

        When(x => x.StructureSetup is not null, () =>
        {
            // FE must send at least one floor with final room rows after any quick-apply UI work.
            RuleFor(x => x.StructureSetup.Floors)
                .NotEmptyCollection();

            RuleForEach(x => x.StructureSetup.Floors)
                .SetValidator(new CreatePropertyFloorRequestDtoValidator());
        });

        // Property-level charge policy rows are optional, but each supplied row must validate independently.
        RuleFor(x => x.ChargePolicies).MaxCount(MAX_CHARGE_POLICIES);

        RuleForEach(x => x.ChargePolicies)
            .SetValidator(new CreatePropertyChargePolicyRequestDtoValidator());

        // Package templates are optional common property-level setup; room-level overrides are handled by room flows.
        RuleFor(x => x.Packages).MaxCount(MAX_PACKAGE_TEMPLATES);

        RuleForEach(x => x.Packages)
            .SetValidator(new CreatePropertyPackageRequestDtoValidator());
    }

    /// <summary>
    /// Ensures the submitted room rows stay inside the V1 safety limit.
    /// </summary>
    /// <param name="command">The create property command.</param>
    /// <returns><c>true</c> when the submitted room count is within the accepted limit.</returns>
    private static bool StayWithinSubmittedRoomLimit(CreatePropertyCommand command)
    {
        if (command.StructureSetup is null)
        {
            return false;
        }

        // Keep writes bounded so one UI action cannot create an unexpectedly large graph.
        var unitCount = command.StructureSetup.Floors.Sum(floor => (floor.Rooms ?? []).Count);

        return unitCount <= MAX_PROPERTY_STRUCTURE_ROOMS;
    }

}

/// <summary>
/// Validates one explicit floor in a property setup request.
/// </summary>
public class CreatePropertyFloorRequestDtoValidator : AbstractValidator<CreatePropertyFloorRequestDto>
{
    /// <summary>
    /// Creates validation rules for an explicit floor request.
    /// </summary>
    public CreatePropertyFloorRequestDtoValidator()
    {
        // Explicit floors still use the no-Floors-table model, so the number must be valid for Units.FloorNumber.
        RuleFor(x => x.FloorNumber)
            .BetweenInclusive(1, MAX_PROPERTY_STRUCTURE_FLOORS);

        RuleFor(x => x.Rooms)
            .NotEmptyCollection();

        RuleForEach(x => x.Rooms)
            .SetValidator(new CreatePropertyRoomRequestDtoValidator());
    }
}

/// <summary>
/// Validates one explicit room in a property setup request.
/// </summary>
public class CreatePropertyRoomRequestDtoValidator : AbstractValidator<CreatePropertyRoomRequestDto>
{
    /// <summary>
    /// Creates validation rules for an explicit room request.
    /// </summary>
    public CreatePropertyRoomRequestDtoValidator()
    {
        // FE sends display data plus combobox codes; backend generates the persisted UnitCode.
        RuleFor(x => x.Name).Required().MaxLen(255);

        RuleFor(x => x.TypeCode).Required().MaxLen(100);

        RuleFor(x => x.RentalModeCode).Required().MaxLen(100);

        RuleFor(x => x.AreaSqm)
            .GreaterThanZeroWhenPresent();

        RuleFor(x => x.BaseRentAmount)
            .GreaterOrEqualZeroWhenPresent();

        RuleFor(x => x.DefaultDepositAmount)
            .GreaterOrEqualZeroWhenPresent();

        RuleFor(x => x.TotalBeds)
            .Required()
            .GreaterThanZero()
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
    }
}

/// <summary>
/// Validates a property-level rental charge policy request.
/// </summary>
public class CreatePropertyChargePolicyRequestDtoValidator : AbstractValidator<CreatePropertyChargePolicyRequestDto>
{
    /// <summary>
    /// Creates validation rules for a charge policy request.
    /// </summary>
    public CreatePropertyChargePolicyRequestDtoValidator()
    {
        // Charge type and amount are the persisted core of a V1 property-level policy.
        RuleFor(x => x.Code).Required().MaxLen(100);

        RuleFor(x => x.Name).MaxLen(255);

        RuleFor(x => x.Amount).Required().GreaterOrEqualZero();

        // Calculation method maps to the persisted IsUsageBased flag, so it must be explicit.
        RuleFor(x => x.CalculationMethodCode).Required().MaxLen(100);

        RuleFor(x => x.VehicleTypeCode).MaxLen(100);

        When(
            policy => string.Equals(
                policy.Code,
                MASTER_CODE_INVOICE_LINE_TYPE_PARKING,
                StringComparison.OrdinalIgnoreCase),
            () =>
        {
            // Parking policies need a vehicle type so generated bill lines can pick the right parking fee later.
            RuleFor(x => x.VehicleTypeCode).Required().MaxLen(100);
        });

        When(
            policy => !string.Equals(
                policy.Code,
                MASTER_CODE_INVOICE_LINE_TYPE_PARKING,
                StringComparison.OrdinalIgnoreCase),
            () =>
        {
            // Non-parking charge policies must not carry parking-only vehicle-type data.
            RuleFor(x => x.VehicleTypeCode).Empty();
        });
    }
}

/// <summary>
/// Validates a service/furniture package request.
/// </summary>
public class CreatePropertyPackageRequestDtoValidator : AbstractValidator<CreatePropertyPackageRequestDto>
{
    /// <summary>
    /// Creates validation rules for a package request.
    /// </summary>
    public CreatePropertyPackageRequestDtoValidator()
    {
        // The package card shown in room setup needs a name and non-negative price adjustment.
        RuleFor(x => x.Name).Required().MaxLen(255);

        RuleFor(x => x.PriceAdjustment).GreaterOrEqualZero();

        // Package item count is bounded because each item becomes one persisted package-item row.
        RuleFor(x => x.Items).MaxCount(MAX_PACKAGE_ITEMS);

        RuleForEach(x => x.Items)
            .SetValidator(new CreatePropertyPackageItemRequestDtoValidator());
    }
}

/// <summary>
/// Validates a service/furniture package item request.
/// </summary>
public class CreatePropertyPackageItemRequestDtoValidator : AbstractValidator<CreatePropertyPackageItemRequestDto>
{
    /// <summary>
    /// Creates validation rules for a package item request.
    /// </summary>
    public CreatePropertyPackageItemRequestDtoValidator()
    {
        // Package items are simple labels such as furniture or included service names.
        RuleFor(x => x.Name).Required().MaxLen(255);
    }
}
