namespace Haven.Application.Commands.DeleteRoomPackage;

/// <summary>
/// Represents a request to soft-delete one effective package from a room.
/// </summary>
/// <param name="PackagePublicId">The frontend-safe package identifier.</param>
/// <param name="RoomPublicId">The room context used to resolve the effective package.</param>
public sealed record DeleteRoomPackageCommand(Guid PackagePublicId, Guid RoomPublicId)
    : ICommand<ResponseDto<OperationStatusResponseDto>>;
