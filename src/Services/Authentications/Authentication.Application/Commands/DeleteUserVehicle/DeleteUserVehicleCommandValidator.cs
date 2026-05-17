namespace Authentication.Application.Commands.DeleteUserVehicle;

/// <summary>
/// Validates tenant profile vehicle delete requests.
/// </summary>
public class DeleteUserVehicleCommandValidator : AbstractValidator<DeleteUserVehicleCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteUserVehicleCommandValidator"/> class.
    /// </summary>
    public DeleteUserVehicleCommandValidator()
    {
        RuleFor(x => x.VehiclePublicId).NotEmpty();
    }
}
