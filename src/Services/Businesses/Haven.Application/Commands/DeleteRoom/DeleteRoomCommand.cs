namespace Haven.Application.Commands.DeleteRoom;

/// <summary>
/// Represents a request to delete one room.
/// </summary>
/// <param name="RoomPublicId">The room public identifier from the route.</param>
public sealed record DeleteRoomCommand(Guid RoomPublicId) : ICommand<ResponseDto<OperationStatusResponseDto>>;
