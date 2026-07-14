namespace Haven.Application.Queries.GetRoomDetail;

/// <summary>
/// Represents a room detail query.
/// </summary>
/// <param name="RoomPublicId">The room public identifier from the route.</param>
public sealed record GetRoomDetailQuery(Guid RoomPublicId) : IQuery<ResponseDto<RoomDetailResponseDto>>;
