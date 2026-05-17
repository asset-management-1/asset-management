namespace Authentication.Application.Commands.DeleteUserVehicle;

/// <summary>
/// Represents a request to remove a vehicle from the current tenant profile.
/// </summary>
public class DeleteUserVehicleCommand : ICommand<ResponseDto<OperationStatusResponseDto>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteUserVehicleCommand"/> class.
    /// </summary>
    /// <param name="vehiclePublicId">The public vehicle identifier from the route.</param>
    public DeleteUserVehicleCommand(Guid vehiclePublicId)
    {
        VehiclePublicId = vehiclePublicId;
    }

    /// <summary>
    /// Gets the public vehicle identifier to remove.
    /// </summary>
    public Guid VehiclePublicId { get; }
}
