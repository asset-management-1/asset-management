namespace Be.Haven.ApiCommon.Services;

/// <summary>
/// Validates Haven access tokens against the per-user auth reset marker.
/// </summary>
public class HavenAuthResetValidator : IAuthResetValidator
{
    private readonly ICachingService _cachingService;
    private readonly IDapperService _dapperService;
    private readonly AuthenticationTokenValidationOptions _authOptions;
    private readonly ILogger<HavenAuthResetValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HavenAuthResetValidator"/> class.
    /// </summary>
    /// <param name="cachingService">The cache service used for fast auth reset marker lookups.</param>
    /// <param name="dapperService">The Dapper service used for source-of-truth fallback reads.</param>
    /// <param name="authOptions">The shared authentication options.</param>
    /// <param name="logger">The validator logger.</param>
    public HavenAuthResetValidator(
        ICachingService cachingService,
        IDapperService dapperService,
        IOptions<AuthenticationTokenValidationOptions> authOptions,
        ILogger<HavenAuthResetValidator> logger)
    {
        // Store collaborators used by the cache-first and DB-fallback validation path.
        _cachingService = cachingService;
        _dapperService = dapperService;
        _authOptions = authOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Validates one access token issue timestamp against the current user auth reset marker.
    /// </summary>
    /// <param name="userPublicId">The public user identifier from the validated token.</param>
    /// <param name="issuedAtMs">The token issue timestamp in Unix milliseconds.</param>
    /// <param name="cancellationToken">The token used to cancel cache or database operations.</param>
    /// <returns><c>true</c> when the token remains valid; otherwise <c>false</c>.</returns>
    public async Task<bool> IsTokenValidAsync(
        Guid userPublicId,
        long issuedAtMs,
        CancellationToken cancellationToken = default)
    {
        // Redis is the fast path; missing or unreadable cache falls back to the DB source of truth.
        var cacheKey = TokenHelper.BuildAuthResetCacheKey(userPublicId);
        try
        {
            var cachedAuthResetAtMs = await _cachingService.GetAsync<long?>(cacheKey, cancellationToken);
            if (cachedAuthResetAtMs.HasValue)
            {
                // Cache hit keeps protected requests off the database hot path.
                return IsIssuedAfterReset(issuedAtMs, cachedAuthResetAtMs.Value);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, HavenAuthenticationLogs.LOG_AUTH_RESET_CACHE_READ_FAILED, userPublicId);
        }

        // Cache miss/error is resolved by a minimal DB read, then cached for subsequent protected requests.
        var authResetAtMs = await LoadAuthResetAtFromDbAsync(userPublicId, cancellationToken);
        await TrySeedCacheAsync(cacheKey, authResetAtMs, userPublicId, cancellationToken);

        return IsIssuedAfterReset(issuedAtMs, authResetAtMs);
    }

    /// <summary>
    /// Loads the auth reset marker from the database source of truth.
    /// </summary>
    /// <param name="userPublicId">The public user identifier from the validated token.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The auth reset timestamp in Unix milliseconds, or <c>0</c> when the user has never reset auth.</returns>
    private async Task<long> LoadAuthResetAtFromDbAsync(
        Guid userPublicId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Only AuthResetAt is needed for token validation, so the query does not load full user rows.
            var readModel = await _dapperService.QueryFirstOrDefaultAsync<AuthResetReadModel>(
                HavenAuthResetConstants.GET_AUTH_RESET_AT_BY_PUBLIC_ID_QUERY,
                new { UserPublicId = userPublicId },
                DapperCommandOptionsHelper.CreateText(cancellationToken));

            if (readModel is null)
            {
                // A deleted or missing user must invalidate the already-validated token.
                return -1;
            }

            // Null AuthResetAt becomes zero, which means no reset has happened yet.
            return TokenHelper.ToAuthResetUnixMilliseconds(readModel.AuthResetAt);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, HavenAuthenticationLogs.LOG_AUTH_RESET_DB_LOOKUP_FAILED, userPublicId);
            throw new HttpStatusCodeException(
                AUTH_STATE_UNAVAILABLE,
                SERVICE_UNAVAILABLE,
                StatusCodes.Status503ServiceUnavailable);
        }
    }

    /// <summary>
    /// Seeds Redis with the resolved auth reset marker for future validations.
    /// </summary>
    /// <param name="cacheKey">The auth reset cache key.</param>
    /// <param name="authResetAtMs">The auth reset timestamp in Unix milliseconds.</param>
    /// <param name="userPublicId">The public user identifier used for safe logging.</param>
    /// <param name="cancellationToken">The token used to cancel the cache operation.</param>
    /// <returns>A task that completes when best-effort cache seeding finishes.</returns>
    private async Task TrySeedCacheAsync(
        string cacheKey,
        long authResetAtMs,
        Guid userPublicId,
        CancellationToken cancellationToken)
    {
        try
        {
            // A seed failure should not block a valid DB-backed authentication decision.
            await _cachingService.SetAbsoluteAsync(
                cacheKey,
                authResetAtMs,
                TokenHelper.GetAuthResetCacheTtl(_authOptions.RefreshTokenDays),
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, HavenAuthenticationLogs.LOG_AUTH_RESET_CACHE_SEED_FAILED, userPublicId);
        }
    }

    /// <summary>
    /// Determines whether the token issue timestamp is still valid after an auth reset marker.
    /// </summary>
    /// <param name="issuedAtMs">The token issue timestamp in Unix milliseconds.</param>
    /// <param name="authResetAtMs">The auth reset marker in Unix milliseconds.</param>
    /// <returns><c>true</c> when the token was issued after the reset marker; otherwise <c>false</c>.</returns>
    private static bool IsIssuedAfterReset(long issuedAtMs, long authResetAtMs)
    {
        // A negative marker means the token user no longer exists; zero means no auth reset was recorded.
        return authResetAtMs >= 0
               && (authResetAtMs == 0 || issuedAtMs > authResetAtMs);
    }
}
