namespace Haven.Application.Commands.CreateVehicle;

/// <summary>
/// Validates landlord vehicle registration input.
/// </summary>
public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    /// <summary>
    /// Creates validation rules for vehicle registration.
    /// </summary>
    public CreateVehicleCommandValidator()
    {
        // Room, payer, and vehicle type are persisted relationships selected through the UI.
        RuleFor(x => x.RoomId).RequiredGuid();
        RuleFor(x => x.PayerTenantId).RequiredGuid();
        RuleFor(x => x.VehicleTypeCode).Required().MaxLen(100);

        // Vehicle text is submitted from the registration form and stored as entered.
        RuleFor(x => x.Name).Required().MaxLen(255);
        RuleFor(x => x.LicensePlate).MaxLen(50);

        // Image metadata is screened here; Core decodes the actual content before storage accepts it.
        RuleFor(x => x.RegistrationFrontImage).OptionalImageFile();
        RuleFor(x => x.RegistrationSideImage).OptionalImageFile();
        RuleFor(x => x.VehicleFrontImage).OptionalImageFile();
        RuleFor(x => x.VehicleSideImage).OptionalImageFile();
    }
}
