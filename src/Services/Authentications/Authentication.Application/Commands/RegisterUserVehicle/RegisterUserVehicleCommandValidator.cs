namespace Authentication.Application.Commands.RegisterUserVehicle;

/// <summary>
/// Validates tenant profile vehicle registration requests.
/// </summary>
public class RegisterUserVehicleCommandValidator : AbstractValidator<RegisterUserVehicleCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserVehicleCommandValidator"/> class.
    /// </summary>
    public RegisterUserVehicleCommandValidator()
    {
        RuleFor(x => x.VehicleType).IsInEnum();

        RuleFor(x => x.VehicleName).Required().MaximumLength(255);

        RuleFor(x => x.LicensePlate).MaximumLength(50);

        RuleFor(x => x.FrontFile)
            .Must(x => x is null || x.Length > 0)
            .WithMessage(VEHICLE_FILE_EMPTY_MESSAGE);

        RuleFor(x => x.SideFile)
            .Must(x => x is null || x.Length > 0)
            .WithMessage(VEHICLE_FILE_EMPTY_MESSAGE);
    }
}
