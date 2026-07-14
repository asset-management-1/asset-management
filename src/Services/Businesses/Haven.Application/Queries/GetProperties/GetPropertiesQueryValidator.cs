namespace Haven.Application.Queries.GetProperties;

/// <summary>
/// Validates property list filters and pagination.
/// </summary>
public class GetPropertiesQueryValidator : AbstractValidator<GetPropertiesQuery>
{
    /// <summary>
    /// Creates validation rules for the property list query.
    /// </summary>
    public GetPropertiesQueryValidator()
    {
        // Paging must stay positive and capped because this query can fan out into child floor and room reads.
        RuleFor(x => x.PageNumber).GreaterThanZero();

        RuleFor(x => x.PageSize).BetweenInclusive(1, MAX_PROPERTY_PAGE_SIZE);

        // Optional numeric filters are validated only when supplied.
        RuleFor(x => x.FloorNumber).GreaterThanZeroWhenPresent();

        // Search and code filters are bounded so query hashing and SQL parameters stay predictable.
        RuleFor(x => x.Search).MaxLen(255);

        RuleFor(x => x.PropertyTypeCode).MaxLen(100);

        RuleFor(x => x.StatusCode).MaxLen(100);

        RuleFor(x => x.RoomStatusCode).MaxLen(100);

        RuleFor(x => x.PaymentStatusCode).MaxLen(100);
    }
}