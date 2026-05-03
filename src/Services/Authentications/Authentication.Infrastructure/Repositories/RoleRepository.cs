namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for roles.
/// </summary>
public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    private readonly AuthenticationDbContext _authenticationDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public RoleRepository(AuthenticationDbContext dbContext) : base(dbContext)
    {
        _authenticationDbContext = dbContext;
    }

    /// <summary>
    /// Gets the default role mapped to a party type.
    /// </summary>
    /// <param name="partyType">The party type code or name, such as Tenant or Landlord.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched role with active role permissions; otherwise <c>null</c>.</returns>
    public Task<Role> GetByPartyTypeAsync(
        string partyType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(partyType))
        {
            return Task.FromResult<Role>(null);
        }

        var normalizedPartyType = partyType.Trim();

        // Load the role and its active permissions to assign default authorization for the party type.
        return _authenticationDbContext.Roles
                                       .AsNoTracking()
                                       .Include(x => x.RolePermissions.Where(y => !y.IsDeleted))
                                       .ThenInclude(x => x.Permission)
                                       .FirstOrDefaultAsync(
                                           x => !x.IsDeleted
                                                && (x.Code == normalizedPartyType || x.Name == normalizedPartyType),
                                           cancellationToken);
    }
}