namespace Haven.Application.Queries.GetTenants;

/// <summary>
/// Validates tenant list queries.
/// </summary>
public class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
    /// <summary>
    /// Creates validation rules for tenant list filters and pagination.
    /// </summary>
    public GetTenantsQueryValidator()
    {
        // Optional id filters must be real frontend-safe identifiers when supplied.
        RuleFor(x => x.PropertyId).NotDefaultWhenPresent();
        RuleFor(x => x.RoomId).NotDefaultWhenPresent();

        // Page controls stay bounded because tenant list joins property, room, contract, and occupancy data.
        RuleFor(x => x.PageNumber).GreaterThanZero();
        RuleFor(x => x.PageSize).BetweenInclusive(1, MAX_ROOM_PAGE_SIZE);

        // Textbox search stays bounded so SQL parameters remain predictable.
        RuleFor(x => x.Search).MaxLen(255);

        // Role filter is optional, but a supplied role must match the room membership flow.
        RuleFor(x => x.RoleCode)
            .MaxLen(100)
            .Must(roleCode => string.IsNullOrWhiteSpace(roleCode)
                              || string.Equals(roleCode, TENANT_ROLE_PRIMARY, StringComparison.OrdinalIgnoreCase)
                              || string.Equals(roleCode, TENANT_ROLE_OCCUPANT, StringComparison.OrdinalIgnoreCase))
            .WithMessage(ApplicationErrorConstants.TenantErrors.ERROR_TENANT_ROLE_INVALID);

        // Status filter remains a compact master-data code resolved by the query.
        RuleFor(x => x.OccupancyStatusCode).MaxLen(100);
    }
}
