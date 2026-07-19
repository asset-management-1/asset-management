namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for refresh tokens.
/// </summary>
public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly AuthenticationDbContext _authenticationDbContext;
    private readonly IDapperService _dapperService;

    /// <summary>
    /// Creates the refresh-token repository with EF writes and transaction-aware Dapper row locks.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    /// <param name="dapperService">The Dapper service used to execute row-lock queries on the EF transaction.</param>
    public RefreshTokenRepository(
        AuthenticationDbContext dbContext,
        IDapperService dapperService) : base(dbContext)
    {
        _authenticationDbContext = dbContext;
        _dapperService = dapperService;
    }

    /// <summary>
    /// Gets the client-session refresh token for one user and client instance.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="deviceId">The normalised client instance identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked refresh token row when it exists; otherwise <c>null</c>.</returns>
    public Task<RefreshToken> GetClientSessionByUserAndDeviceIdAsync(
        long userId,
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        // A missing client instance id cannot identify a stable client session.
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return Task.FromResult<RefreshToken>(null);
        }

        // Login updates the tracked row in place to prevent one-row-per-login spam for the same client instance.
        return _authenticationDbContext.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.UserId == userId
                     && x.DeviceId == deviceId
                     && !x.IsDeleted,
                cancellationToken);
    }

    /// <summary>
    /// Loads and locks the refresh session matching either its current hash or immediate previous hash.
    /// </summary>
    /// <param name="refreshTokenHash">The hashed refresh credential.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked locked session with its owning account when found; otherwise <c>null</c>.</returns>
    public async Task<RefreshToken> GetForRefreshForUpdateAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default)
    {
        // Empty hashes cannot identify a refresh session and must not reach SQL.
        if (string.IsNullOrWhiteSpace(refreshTokenHash))
        {
            return null;
        }

        // Step 1: Lock the matching row identifier on the required EF rotation transaction.
        var refreshTokenId = await _dapperService.ExecuteScalarAsync<long?>(
            InfrastructureQueryConstants.GET_REFRESH_TOKEN_ID_FOR_UPDATE_QUERY,
            new { RefreshTokenHash = refreshTokenHash },
            DapperCommandOptionsHelper.CreateTransactionalText(
                _authenticationDbContext,
                cancellationToken));
        if (!refreshTokenId.HasValue)
        {
            return null;
        }

        // Step 3: Load the locked row as a tracked EF entity with the account status needed by refresh validation.
        return await _authenticationDbContext.RefreshTokens
            .Include(x => x.User)
                .ThenInclude(x => x.Status)
            .FirstOrDefaultAsync(x => x.Id == refreshTokenId.Value, cancellationToken);
    }

    /// <summary>
    /// Loads the active refresh-token session identified by its owner and public session identifier.
    /// </summary>
    /// <param name="userId">The internal user identifier that owns the session.</param>
    /// <param name="sessionPublicId">The public session identifier carried by the access token.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The active tracked refresh-token session when found; otherwise, <c>null</c>.</returns>
    public Task<RefreshToken> GetByUserAndSessionPublicIdAsync(
        long userId,
        Guid sessionPublicId,
        CancellationToken cancellationToken = default)
    {
        // Session-scoped mutations must update only the refresh-token row named by the access-token sid claim.
        return _authenticationDbContext.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.UserId == userId
                     && x.SessionPublicId == sessionPublicId
                     && !x.IsDeleted
                     && !x.RevokedAt.HasValue
                     && x.ExpiresAt > DateTime.UtcNow,
                cancellationToken);
    }

    /// <summary>
    /// Loads and locks one active refresh session for authenticated credential rotation.
    /// </summary>
    /// <param name="userId">The internal user identifier that owns the session.</param>
    /// <param name="sessionPublicId">The public session identifier carried by the access token.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked locked active session when found; otherwise <c>null</c>.</returns>
    public async Task<RefreshToken> GetByUserAndSessionPublicIdForUpdateAsync(
        long userId,
        Guid sessionPublicId,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Lock the active session identifier on the required credential-change transaction.
        var refreshTokenId = await _dapperService.ExecuteScalarAsync<long?>(
            InfrastructureQueryConstants.GET_ACTIVE_REFRESH_TOKEN_ID_BY_SESSION_FOR_UPDATE_QUERY,
            new { UserId = userId, SessionPublicId = sessionPublicId },
            DapperCommandOptionsHelper.CreateTransactionalText(
                _authenticationDbContext,
                cancellationToken));
        if (!refreshTokenId.HasValue)
        {
            return null;
        }

        // Step 3: Materialise the locked session through EF so subsequent rotation mutates tracked state.
        return await _authenticationDbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Id == refreshTokenId.Value, cancellationToken);
    }

    /// <summary>
    /// Stages one refresh-token revocation by marking only revocation columns as modified.
    /// </summary>
    /// <param name="refreshTokenId">The internal refresh-token identifier.</param>
    /// <param name="revokedAt">The UTC revocation timestamp.</param>
    /// <param name="replacedByTokenHash">The replacement token hash when token rotation is happening.</param>
    /// <returns>A completed task after the field-level update is staged.</returns>
    public Task StageRevocationAsync(
        long refreshTokenId,
        DateTime revokedAt,
        string replacedByTokenHash = null)
    {
        // Attach a stub token so EF marks only revocation fields as modified.
        var refreshToken = new RefreshToken
        {
            Id = refreshTokenId,
            RevokedAt = revokedAt,
            ReplacedByTokenHash = replacedByTokenHash
        };

        _authenticationDbContext.RefreshTokens.Attach(refreshToken);
        _authenticationDbContext.Entry(refreshToken).Property(x => x.RevokedAt).IsModified = true;
        if (!string.IsNullOrWhiteSpace(replacedByTokenHash))
        {
            _authenticationDbContext.Entry(refreshToken).Property(x => x.ReplacedByTokenHash).IsModified = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stages revocation for all active refresh tokens owned by one user.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="revokedAt">The UTC revocation timestamp.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>A task that completes after all field-level revocation updates are staged.</returns>
    public async Task StageActiveRevocationsByUserIdAsync(
        long userId,
        DateTime revokedAt,
        CancellationToken cancellationToken = default)
    {
        // Capture one timestamp for the active-token filter used by this revocation batch.
        var now = DateTime.UtcNow;

        // Load ids only, then stage field-level updates without materializing full token rows.
        var refreshTokenIds = await _authenticationDbContext.RefreshTokens
            .AsNoTracking()
            .Where(x => x.UserId == userId
                        && !x.IsDeleted
                        && !x.RevokedAt.HasValue
                        && x.ExpiresAt > now)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var refreshTokenId in refreshTokenIds)
        {
            // Stage each token through the same revocation helper to keep modified columns consistent.
            await StageRevocationAsync(refreshTokenId, revokedAt);
        }
    }

    /// <summary>
    /// Stages revocation of every active user session except the authenticated current session.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="currentSessionPublicId">The current session that must remain active for rotation.</param>
    /// <param name="revokedAt">The UTC revocation timestamp.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>A task that completes after all other active sessions are staged for revocation.</returns>
    public async Task StageActiveRevocationsExceptSessionAsync(
        long userId,
        Guid currentSessionPublicId,
        DateTime revokedAt,
        CancellationToken cancellationToken = default)
    {
        // Capture one timestamp so every active-session filter in this batch uses the same boundary.
        var now = DateTime.UtcNow;

        // Select only other active session ids without materializing their full tracked entities.
        var refreshTokenIds = await _authenticationDbContext.RefreshTokens
            .AsNoTracking()
            .Where(x => x.UserId == userId
                        && x.SessionPublicId != currentSessionPublicId
                        && !x.IsDeleted
                        && !x.RevokedAt.HasValue
                        && x.ExpiresAt > now)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        // Stage field-level revocations through the shared helper so modified columns remain consistent.
        foreach (var refreshTokenId in refreshTokenIds)
        {
            await StageRevocationAsync(refreshTokenId, revokedAt);
        }
    }

    /// <summary>
    /// Stages active refresh-token revocations for one server-issued session without loading full token rows.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="sessionPublicId">The server-issued public session identifier from the access token.</param>
    /// <param name="revokedAt">The UTC revocation timestamp.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>A task that completes after all field-level revocation updates are staged.</returns>
    public async Task StageActiveRevocationsByUserAndSessionPublicIdAsync(
        long userId,
        Guid sessionPublicId,
        DateTime revokedAt,
        CancellationToken cancellationToken = default)
    {
        // Capture one timestamp for the active-token filter used by this revocation batch.
        var now = DateTime.UtcNow;

        // Load ids only, then stage field-level updates without materializing full token rows.
        var refreshTokenIds = await _authenticationDbContext.RefreshTokens
            .AsNoTracking()
            .Where(x => x.UserId == userId
                        && x.SessionPublicId == sessionPublicId
                        && !x.IsDeleted
                        && !x.RevokedAt.HasValue
                        && x.ExpiresAt > now)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var refreshTokenId in refreshTokenIds)
        {
            // Stage each token through the same revocation helper to keep modified columns consistent.
            await StageRevocationAsync(refreshTokenId, revokedAt);
        }
    }
}
