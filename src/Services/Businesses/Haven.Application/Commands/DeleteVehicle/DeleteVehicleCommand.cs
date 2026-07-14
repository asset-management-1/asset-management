namespace Haven.Application.Commands.DeleteVehicle;

/// <summary>
/// Represents a request to soft-delete one vehicle.
/// </summary>
public sealed record DeleteVehicleCommand(Guid RoomId, Guid VehiclePublicId)
    : ICommand<ResponseDto<OperationStatusResponseDto>>;
