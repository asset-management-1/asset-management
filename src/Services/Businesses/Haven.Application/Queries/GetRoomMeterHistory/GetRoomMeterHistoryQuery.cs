namespace Haven.Application.Queries.GetRoomMeterHistory;

/// <summary>
/// Represents a bounded room meter history query for one calendar year.
/// </summary>
/// <param name="RoomId">The room identifier supplied by the route.</param>
/// <param name="Year">The selected calendar year.</param>
public sealed record GetRoomMeterHistoryQuery(Guid RoomId, int Year)
    : IQuery<ResponseDto<IReadOnlyList<MeterHistoryItemResponseDto>>>;
