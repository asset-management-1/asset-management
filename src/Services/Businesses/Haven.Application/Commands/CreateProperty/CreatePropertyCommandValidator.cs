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
        RuleFor(x => x.Name).Required().MaximumLength(255);

        RuleFor(x => x.PropertyTypeCode).Required().MaximumLength(100);

        // Location fields are optional in V1, but supplied values must stay within expected code/address lengths.
        RuleFor(x => x.ProvinceCode).MaximumLength(30);

        RuleFor(x => x.DistrictCode).MaximumLength(30);

        RuleFor(x => x.WardCode).MaximumLength(30);

        RuleFor(x => x.StreetAddress).MaximumLength(1000);

        RuleFor(x => x.FormattedAddress).MaximumLength(1000);

        // Coordinates are optional and validated only when the user supplies map data.
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);

        // Structure setup drives generated Units, so the object and total write size are validated up front.
        RuleFor(x => x.StructureSetup).NotNull();

        RuleFor(x => x).Must(StayWithinGenerationLimit)
            .WithMessage($"Generated units must be less than or equal to {MAX_GENERATED_UNITS}.");

        When(x => x.StructureSetup is not null, () =>
        {
            When(x => !HasExplicitStructure(x.StructureSetup), () =>
            {
                // Quick setup bounds generated rooms before unit code generation and EF graph creation.
                RuleFor(x => x.StructureSetup.TotalFloors)
                    .InclusiveBetween(1, MAX_GENERATED_FLOORS);

                RuleFor(x => x.StructureSetup.RoomsPerFloor)
                    .InclusiveBetween(1, MAX_GENERATED_ROOMS_PER_FLOOR);

                RuleFor(x => x.StructureSetup.RoomNumberingPattern)
                    .MaximumLength(50);

                // Default master-data codes must be present for generated unit setup.
                RuleFor(x => x.StructureSetup.DefaultUnitTypeCode)
                    .Required()
                    .MaximumLength(100);

                RuleFor(x => x.StructureSetup.DefaultRentalModeCode)
                    .Required()
                    .MaximumLength(100);

                // Optional default numeric values are validated only when the setup supplies them.
                RuleFor(x => x.StructureSetup.DefaultAreaSqm)
                    .GreaterThan(0)
                    .When(x => x.StructureSetup.DefaultAreaSqm.HasValue);

                RuleFor(x => x.StructureSetup.DefaultBaseRentAmount)
                    .GreaterThanOrEqualTo(0)
                    .When(x => x.StructureSetup.DefaultBaseRentAmount.HasValue);

                RuleFor(x => x.StructureSetup.DefaultDepositAmount)
                    .GreaterThanOrEqualTo(0)
                    .When(x => x.StructureSetup.DefaultDepositAmount.HasValue);

                RuleFor(x => x.StructureSetup.DefaultTotalBeds)
                    .NotNull()
                    .GreaterThan(0)
                    .When(x => CreatePropertyValidationHelper.IsSharedBedRentalMode(x.StructureSetup.DefaultRentalModeCode));

                RuleFor(x => x.StructureSetup.DefaultTotalBeds)
                    .Empty()
                    .When(x => CreatePropertyValidationHelper.IsNonSharedBedRentalMode(x.StructureSetup.DefaultRentalModeCode));
            });

            When(x => HasExplicitStructure(x.StructureSetup), () =>
            {
                // Explicit setup uses FE-composed floor/room rows instead of uniform generated rooms.
                RuleForEach(x => x.StructureSetup.Floors)
                    .SetValidator(new CreatePropertyFloorRequestDtoValidator());

                RuleFor(x => x)
                    .Must(HaveUniqueExplicitUnitCodes)
                    .WithMessage("Explicit room unit codes must be unique within the property.");
            });
        });

        // Property-level charge policy rows are optional, but each supplied row must validate independently.
        RuleForEach(x => x.ChargePolicies)
            .SetValidator(new CreatePropertyChargePolicyRequestDtoValidator());

        // Package templates are optional; when present they are copied to every generated room.
        RuleForEach(x => x.Packages)
            .SetValidator(new CreatePropertyPackageRequestDtoValidator());
    }

    /// <summary>
    /// Ensures the generated rooms stay inside the V1 safety limit.
    /// </summary>
    /// <param name="command">The create property command.</param>
    /// <returns><c>true</c> when generated room count is within limit.</returns>
    private static bool StayWithinGenerationLimit(CreatePropertyCommand command)
    {
        if (command.StructureSetup is null)
        {
            return false;
        }

        // Keep generated writes bounded so one UI action cannot create an unexpectedly large graph.
        var unitCount = HasExplicitStructure(command.StructureSetup)
            ? command.StructureSetup.Floors.Sum(floor => (floor.Rooms ?? []).Count)
            : command.StructureSetup.TotalFloors * command.StructureSetup.RoomsPerFloor;

        return unitCount <= MAX_GENERATED_UNITS;
    }

    /// <summary>
    /// Checks whether the structure uses explicit UI-composed floor/room rows.
    /// </summary>
    /// <param name="structure">The structure setup.</param>
    /// <returns><c>true</c> when explicit floors are supplied.</returns>
    private static bool HasExplicitStructure(CreatePropertyStructureRequestDto structure)
    {
        return (structure.Floors ?? []).Count > 0;
    }

    /// <summary>
    /// Checks whether explicit room unit codes are unique within the property payload.
    /// </summary>
    /// <param name="command">The create command.</param>
    /// <returns><c>true</c> when room codes are unique.</returns>
    private static bool HaveUniqueExplicitUnitCodes(CreatePropertyCommand command)
    {
        var roomCodes = (command.StructureSetup?.Floors ?? [])
            .SelectMany(floor => floor.Rooms ?? [])
            .Select(room => room.Code.NormalizeOptional())
            .Where(code => code is not null)
            .ToList();

        return roomCodes.Count == roomCodes.Distinct(StringComparer.OrdinalIgnoreCase).Count();
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
            .InclusiveBetween(1, MAX_GENERATED_FLOORS);

        RuleFor(x => x.Rooms)
            .NotEmpty();

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
        // FE sends final room data in one payload; backend validates codes and numeric values before persistence.
        RuleFor(x => x.Code).Required().MaximumLength(50);

        RuleFor(x => x.Name).Required().MaximumLength(255);

        RuleFor(x => x.TypeCode).Required().MaximumLength(100);

        RuleFor(x => x.RentalModeCode).Required().MaximumLength(100);

        RuleFor(x => x.AreaSqm)
            .GreaterThan(0)
            .When(x => x.AreaSqm.HasValue);

        RuleFor(x => x.BaseRentAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.BaseRentAmount.HasValue);

        RuleFor(x => x.DefaultDepositAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DefaultDepositAmount.HasValue);

        RuleFor(x => x.TotalBeds)
            .NotNull()
            .GreaterThan(0)
            .When(x => CreatePropertyValidationHelper.IsSharedBedRentalMode(x.RentalModeCode));

        RuleFor(x => x.TotalBeds)
            .Empty()
            .When(x => CreatePropertyValidationHelper.IsNonSharedBedRentalMode(x.RentalModeCode));
    }
}

/// <summary>
/// Provides shared validation helpers for property creation rental-mode rules.
/// </summary>
internal static class CreatePropertyValidationHelper
{
    /// <summary>
    /// Checks whether a rental mode code represents shared-bed/KTX capacity.
    /// </summary>
    /// <param name="rentalModeCode">The rental mode code supplied by the request.</param>
    /// <returns><c>true</c> when the code is the shared-bed rental mode.</returns>
    public static bool IsSharedBedRentalMode(string rentalModeCode)
    {
        return string.Equals(
            rentalModeCode,
            MASTER_CODE_RENTAL_MODE_SHARED_BED,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether a supplied rental mode is not shared-bed/KTX.
    /// </summary>
    /// <param name="rentalModeCode">The rental mode code supplied by the request.</param>
    /// <returns><c>true</c> when the code is present and is not shared-bed.</returns>
    public static bool IsNonSharedBedRentalMode(string rentalModeCode)
    {
        return !string.IsNullOrWhiteSpace(rentalModeCode)
               && !IsSharedBedRentalMode(rentalModeCode);
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
        RuleFor(x => x.Code).Required().MaximumLength(100);

        RuleFor(x => x.Name).MaximumLength(255);

        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);

        // Calculation method is still accepted because it maps to the persisted IsUsageBased flag.
        RuleFor(x => x.CalculationMethodCode).MaximumLength(100);

        RuleFor(x => x.VehicleTypeCode).MaximumLength(100);

        When(IsParkingPolicy, () =>
        {
            // Parking policies need a vehicle type so generated bill lines can pick the right parking fee later.
            RuleFor(x => x.VehicleTypeCode).Required().MaximumLength(100);
        });

        When(x => !IsParkingPolicy(x), () =>
        {
            // Non-parking charge policies must not carry parking-only vehicle-type data.
            RuleFor(x => x.VehicleTypeCode).Empty();
        });
    }

    /// <summary>
    /// Checks whether the request is a parking charge policy.
    /// </summary>
    /// <param name="policy">The charge policy request.</param>
    /// <returns><c>true</c> when the policy charge type is parking.</returns>
    private static bool IsParkingPolicy(CreatePropertyChargePolicyRequestDto policy)
    {
        return string.Equals(
            policy.Code,
            MASTER_CODE_INVOICE_LINE_TYPE_PARKING,
            StringComparison.OrdinalIgnoreCase);
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
        RuleFor(x => x.Name).Required().MaximumLength(255);

        RuleFor(x => x.PriceAdjustment).GreaterThanOrEqualTo(0);

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
        RuleFor(x => x.Name).Required().MaximumLength(255);
    }
}
