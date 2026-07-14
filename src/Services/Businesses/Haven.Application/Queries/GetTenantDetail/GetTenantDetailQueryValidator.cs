namespace Haven.Application.Queries.GetTenantDetail;

/// <summary>
/// Validates tenant occupancy detail route input.
/// </summary>
public class GetTenantDetailQueryValidator : AbstractValidator<GetTenantDetailQuery>
{
    /// <summary>
    /// Creates validation rules for tenant detail queries.
    /// </summary>
    public GetTenantDetailQueryValidator()
    {
        // Tenant detail is scoped by an occupancy public id from the route.
        RuleFor(x => x.Id).RequiredGuid();
    }
}




