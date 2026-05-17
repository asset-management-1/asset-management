namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for external login records.
/// </summary>
public class ExternalLoginRepository : GenericRepository<ExternalLogin>, IExternalLoginRepository
{
    private readonly AuthenticationDbContext _authenticationDbContext;

    /// <summary>
    /// Creates the external-login repository with EF tracking access for provider-link writes.
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
    /// <returns>The matched external login with the user status required for authentication; otherwise <c>null</c>.</returns>
    public Task<ExternalLogin> GetByProviderAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default)
    {
        // Project the provider mapping plus minimal user authentication state for external login.
        return _authenticationDbContext.ExternalLogins
            .AsNoTracking()
            .Where(
                x => !x.IsDeleted
                     && x.LoginProvider == provider
                     && x.ProviderKey == providerUserId)
            .Select(x => new ExternalLogin
            {
                Id = x.Id,
                UserId = x.UserId,
                LoginProvider = x.LoginProvider,
                ProviderKey = x.ProviderKey,
                User = new User
                {
                    Id = x.User.Id,
                    PublicId = x.User.PublicId,
                    UserName = x.User.UserName,
                    Email = x.User.Email,
                    AuthResetAt = x.User.AuthResetAt,
                    EmailConfirmed = x.User.EmailConfirmed,
                    IsDeleted = x.User.IsDeleted,
                    Status = new MasterDataValue
                    {
                        Id = x.User.Status.Id,
                        Code = x.User.Status.Code,
                        Name = x.User.Status.Name
                    }
                }
            })
            .FirstOrDefaultAsync(cancellationToken);
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
        // Load the active provider link for link idempotency and conflict checks.
        return _authenticationDbContext.ExternalLogins
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => !x.IsDeleted
                     && x.UserId == userId
                     && x.LoginProvider == provider,
                cancellationToken);
    }

    /// <summary>
    /// Gets a soft-deleted external login by internal user identifier and provider.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="provider">The external login provider, such as Google or Microsoft.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched deleted external login; otherwise <c>null</c>.</returns>
    public Task<ExternalLogin> GetDeletedByUserAndProviderAsync(
        long userId,
        string provider,
        CancellationToken cancellationToken = default)
    {
        // Load soft-deleted links so re-link can revive the previous provider mapping safely.
        return _authenticationDbContext.ExternalLogins
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.IsDeleted
                     && x.UserId == userId
                     && x.LoginProvider == provider,
                cancellationToken);
    }
}
