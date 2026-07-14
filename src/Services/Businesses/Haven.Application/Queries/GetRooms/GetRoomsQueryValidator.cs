namespace Haven.Application.Queries.GetRooms;

/// <summary>
/// Validates room ledger filters and pagination.
/// </summary>
public class GetRoomsQueryValidator : AbstractValidator<GetRoomsQuery>
{
    /// <summary>
    /// Creates validation rules for the room ledger query.
    /// </summary>
    public GetRoomsQueryValidator()
    {
        // Optional property id filter must be a real frontend-safe identifier when supplied.
        RuleFor(x => x.PropertyId).NotDefaultWhenPresent();

        // Page controls stay bounded so one request cannot load too many room cards.
        RuleFor(x => x.PageNumber).GreaterThanZero();
        RuleFor(x => x.PageSize).BetweenInclusive(1, MAX_ROOM_PAGE_SIZE);

        // Optional filters mirror Figma search controls and remain compact code/text values.
        RuleFor(x => x.FloorNumber).GreaterThanZeroWhenPresent();
        RuleFor(x => x.PropertySearch).MaxLen(255);
        RuleFor(x => x.Search).MaxLen(255);
        RuleFor(x => x.RoomStatusCode).MaxLen(100);
        RuleFor(x => x.RentalModeCode).MaxLen(100);
        RuleFor(x => x.PaymentStatusCode).MaxLen(100);
    }
}
