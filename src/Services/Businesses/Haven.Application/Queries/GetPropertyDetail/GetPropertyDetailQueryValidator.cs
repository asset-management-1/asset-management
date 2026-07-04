namespace Haven.Application.Queries.GetPropertyDetail;

/// <summary>
/// Validates the property detail query.
/// </summary>
public class GetPropertyDetailQueryValidator : AbstractValidator<GetPropertyDetailQuery>
{
    /// <summary>
    /// Creates validation rules for the property detail query.
    /// </summary>
    public GetPropertyDetailQueryValidator()
    {
        // Detail reads must always be scoped by a route public identifier.
        RuleFor(x => x.PropertyPublicId).NotEmpty();
    }
}
