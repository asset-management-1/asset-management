namespace Haven.Application.Queries.GetRoomMeterPeriod;

/// <summary>
/// Validates one room meter period query.
/// </summary>
public class GetRoomMeterPeriodQueryValidator : AbstractValidator<GetRoomMeterPeriodQuery>
{
    /// <summary>
    /// Creates route and calendar validation rules.
    /// </summary>
    public GetRoomMeterPeriodQueryValidator()
    {
        // The route identifies the room while month and year identify its selected meter period.
        RuleFor(x => x.RoomId).RequiredGuid();

        RuleFor(x => x.Month).BetweenInclusive(1, 12);

        RuleFor(x => x.Year).BetweenInclusive(2000, 2100);
    }
}
