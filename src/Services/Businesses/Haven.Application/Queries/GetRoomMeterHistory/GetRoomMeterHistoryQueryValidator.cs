namespace Haven.Application.Queries.GetRoomMeterHistory;

/// <summary>
/// Validates room meter history filters.
/// </summary>
public class GetRoomMeterHistoryQueryValidator : AbstractValidator<GetRoomMeterHistoryQuery>
{
    /// <summary>
    /// Creates route and calendar-year validation rules.
    /// </summary>
    public GetRoomMeterHistoryQueryValidator()
    {
        // A valid room and year bound the result to at most twelve monthly periods.
        RuleFor(x => x.RoomId).RequiredGuid();

        RuleFor(x => x.Year).BetweenInclusive(2000, 2100);
    }
}
