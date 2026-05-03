namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for refresh tokens.
/// </summary>
public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly AuthenticationDbContext _authenticationDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public RefreshTokenRepository(AuthenticationDbContext dbContext) : base(dbContext)
    {
        _authenticationDbContext = dbContext;
    }

    /// <summary>
    /// Gets a refresh token for login flow, including user and authorization data.
    /// </summary>
    /// <param name="refreshTokenHash">The hashed refresh token.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched refresh token with user, roles, and permissions; otherwise <c>null</c>.</returns>
    public Task<RefreshToken> GetForLoginAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenHash))
        {
            return Task.FromResult<RefreshToken>(null);
        }

        // Load full user authorization graph to generate access token after refresh.
        return _authenticationDbContext.RefreshTokens
            .AsNoTracking()
            .Include(x => x.User)
                .ThenInclude(x => x.Status)
            .Include(x => x.User)
                .ThenInclude(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                        .ThenInclude(x => x.RolePermissions)
                            .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(
                x => x.TokenHash == refreshTokenHash
                     && !x.IsDeleted,
                cancellationToken);
    }

    /// <summary>
    /// Gets a refresh token by user identifier and token hash.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="refreshTokenHash">The hashed refresh token.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched refresh token; otherwise <c>null</c>.</returns>
    public Task<RefreshToken> GetByUserAndHashAsync(
        long userId,
        string refreshTokenHash,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenHash))
        {
            return Task.FromResult<RefreshToken>(null);
        }

        // Validate that the refresh token belongs to the specific user.
        return _authenticationDbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UserId == userId
                     && x.TokenHash == refreshTokenHash
                     && !x.IsDeleted,
                cancellationToken);
    }

    /// <summary>
    /// Gets all active (non-revoked and non-expired) refresh tokens of a user.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>A list of active refresh tokens.</returns>
    public Task<List<RefreshToken>> GetActiveByUserIdAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // Active tokens must not be deleted, revoked, or expired.
        return _authenticationDbContext.RefreshTokens
            .AsNoTracking()
            .Where(x => x.UserId == userId
                        && !x.IsDeleted
                        && !x.RevokedAt.HasValue
                        && x.ExpiresAt > now)
            .ToListAsync(cancellationToken);
    }
}