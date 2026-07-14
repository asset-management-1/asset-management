namespace Haven.Application.Commands.UpdateVehicle;

/// <summary>
/// Validates partial vehicle update input.
/// </summary>
public class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
    /// <summary>
    /// Creates validation rules for vehicle updates.
    /// </summary>
    public UpdateVehicleCommandValidator()
    {
        // Route identifiers select one vehicle inside its owning room; submitted fields merge into that record.
        RuleFor(x => x.RoomId).RequiredGuid();
        RuleFor(x => x.VehicleId).RequiredGuid();
        RuleFor(x => x.PayerTenantId).NotDefaultWhenPresent();
        RuleFor(x => x.VehicleTypeCode).MaxLen(100);
        RuleFor(x => x.Name).MaxLen(255);
        RuleFor(x => x.LicensePlate).MaxLen(50);

        // Submitted replacement images use the same per-file limit as vehicle creation.
        RuleFor(x => x.RegistrationFrontImage).OptionalImageFile();
        RuleFor(x => x.RegistrationSideImage).OptionalImageFile();
        RuleFor(x => x.VehicleFrontImage).OptionalImageFile();
        RuleFor(x => x.VehicleSideImage).OptionalImageFile();
    }
}
