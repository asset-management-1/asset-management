namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for external login records.
/// </summary>
public class ExternalLoginRepository : GenericRepository<ExternalLogin>, IExternalLoginRepository
{
    private readonly AuthenticationDbContext _authenticationDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalLoginRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public ExternalLoginRepository(AuthenticationDbContext dbContext) : base(dbContext)
    {
        _authenticationDbContext = dbContext;
    }

    /// <summary>
    /// Gets an external login by provider and provider user identifier.
    /// </summary>
    /// <param name="provider">The external login provider, such as Google or Microsoft.</param>
    /// <param name="providerUserId">The unique user identifier returned by the external provider.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched external login with user, status, roles, and permissions; otherwise <c>null</c>.</returns>
    public Task<ExternalLogin> GetByProviderAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default)
    {
        // Load the linked user and authorization data needed to create application tokens after external login.
        return _authenticationDbContext.ExternalLogins
            .AsNoTracking()
            .Include(x => x.User)
                .ThenInclude(x => x.Status)
            .Include(x => x.User)
                .ThenInclude(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                        .ThenInclude(x => x.RolePermissions)
                            .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(
                x => !x.IsDeleted
                     && x.LoginProvider == provider
                     && x.ProviderKey == providerUserId,
                cancellationToken);
    }

    /// <summary>
    /// Gets an external login by internal user identifier and provider.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="provider">The external login provider, such as Google or Microsoft.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched external login; otherwise <c>null</c>.</returns>
    public Task<ExternalLogin> GetByUserAndProviderAsync(
        long userId,
        string provider,
        CancellationToken cancellationToken = default)
    {
        // Check whether the user has already linked this external provider.
        return _authenticationDbContext.ExternalLogins
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => !x.IsDeleted
                     && x.UserId == userId
                     && x.LoginProvider == provider,
                cancellationToken);
    }
}