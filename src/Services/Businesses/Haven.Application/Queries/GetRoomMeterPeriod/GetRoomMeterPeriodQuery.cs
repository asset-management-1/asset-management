namespace Haven.Application.Queries.GetRoomMeterPeriod;

/// <summary>
/// Represents one room meter period query used for display and mutation prefill.
/// </summary>
/// <param name="RoomId">The room identifier supplied by the route.</param>
/// <param name="Month">The selected calendar month.</param>
/// <param name="Year">The selected calendar year.</param>
public sealed record GetRoomMeterPeriodQuery(Guid RoomId, int Month, int Year)
    : IQuery<ResponseDto<MeterPeriodDetailResponseDto>>;
