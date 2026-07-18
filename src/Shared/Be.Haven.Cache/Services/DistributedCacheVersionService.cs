namespace Be.Haven.Cache.Services;

/// <summary>
/// Provides an epoch-scoped cache version used for logical cache invalidation.
/// </summary>
public sealed class DistributedCacheVersionService : ICacheVersionService
{
    private readonly IDatabase _db;
    private readonly ILogger<DistributedCacheVersionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedCacheVersionService"/> class.
    /// </summary>
    /// <param name="redis">Redis connection multiplexer.</param>
    /// <param name="logger">Logger instance.</param>
    public DistributedCacheVersionService(
        IConnectionMultiplexer redis,
        ILogger<DistributedCacheVersionService> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;
    }

    /// <summary>
    /// Returns the current epoch identifier used to scope the version counter.
    /// Default is UTC day formatted as <c>yyyyMMdd</c> (e.g., <c>20260105</c>).
    /// </summary>
    public string GetEpoch() => DateTime.UtcNow.ToString("yyyyMMdd");

    /// <summary>
    /// Retrieves the current cache version for a given cache group, scope, and epoch.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group whose version should be read.</param>
    /// <param name="cacheScope">The optional cache scope within the logical group.</param>
    /// <param name="epoch">The epoch scope for the version counter. Pass a frozen epoch value (captured once) to avoid mismatch at UTC midnight.</param>
    /// <returns>The current version as a long, or <see cref="DEFAULT_VERSION"/> when the key is genuinely missing.</returns>
    public async Task<long> GetAsync(string cacheGroup, string cacheScope, string epoch)
    {
        if (string.IsNullOrWhiteSpace(cacheGroup) || string.IsNullOrWhiteSpace(epoch))
        {
            throw new ArgumentException(CacheVersionLogs.CACHE_VERSION_INVALIDATION_ARGUMENTS_MISSING);
        }

        var key = GetVersionKey(cacheGroup, cacheScope, epoch);

        try
        {
            var v = await _db.StringGetAsync(key);

            // First-time access: key doesn't exist -> default version (1).
            if (!v.HasValue)
            {
                _logger.LogDebug(
                    CacheVersionLogs.LOG_CACHE_VERSION_KEY_NOT_FOUND,
                    epoch,
                    DEFAULT_VERSION);

                return DEFAULT_VERSION;
            }

            // Invalid version state makes cache-key selection unsafe, so let the caller bypass cache.
            if (!long.TryParse(v.ToString(), out var parsed))
            {
                _logger.LogWarning(
                    CacheVersionLogs.LOG_CACHE_VERSION_INVALID_VALUE,
                    epoch);

                throw new InvalidOperationException(string.Format(
                    CacheVersionLogs.CACHE_VERSION_NOT_ADVANCED,
                    cacheGroup,
                    cacheScope,
                    epoch,
                    DEFAULT_VERSION,
                    v.ToString()));
            }

            // Non-positive versions are corrupted state, so bypass cached payloads instead of guessing.
            if (parsed <= 0)
            {
                _logger.LogWarning(
                    CacheVersionLogs.LOG_CACHE_VERSION_NON_POSITIVE_VALUE,
                    epoch,
                    parsed);

                throw new InvalidOperationException(string.Format(
                    CacheVersionLogs.CACHE_VERSION_NOT_ADVANCED,
                    cacheGroup,
                    cacheScope,
                    epoch,
                    DEFAULT_VERSION,
                    parsed));
            }

            _logger.LogDebug(
                CacheVersionLogs.LOG_CACHE_VERSION_RETRIEVED,
                epoch,
                parsed);

            return parsed;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                CacheVersionLogs.LOG_CACHE_VERSION_READ_FAILED,
                epoch);

            throw new InvalidOperationException(string.Format(
                CacheVersionLogs.CACHE_VERSION_READ_UNSAFE,
                cacheGroup,
                cacheScope,
                epoch), ex);
        }
    }

    /// <summary>
    /// Atomically bumps (increments) the cache version for a given cache group, scope, and epoch.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group whose version should be invalidated.</param>
    /// <param name="cacheScope">The optional cache scope within the logical group.</param>
    /// <param name="epoch">The epoch scope for the version counter. Pass a frozen epoch value (captured once) to avoid mismatch at UTC midnight.</param>
    /// <returns>
    /// Returns the new incremented version.
    /// </returns>
    public async Task<long> InvalidateAsync(string cacheGroup, string cacheScope, string epoch)
    {
        if (string.IsNullOrWhiteSpace(cacheGroup) || string.IsNullOrWhiteSpace(epoch))
        {
            throw new ArgumentException(CacheVersionLogs.CACHE_VERSION_INVALIDATION_ARGUMENTS_MISSING);
        }

        var key = GetVersionKey(cacheGroup, cacheScope, epoch);

        // Seed only when absent so concurrent invalidations never reset an existing version.
        var epochLifetime = CacheEpochTtlHelper.GetCurrentEpochLifetime();
        await _db.StringSetAsync(key, DEFAULT_VERSION, epochLifetime, when: When.NotExists);

        var newVersion = await _db.StringIncrementAsync(key);

        // Defensive guard: write invalidation is consistency-critical and must not fail open.
        if (newVersion <= 0)
        {
            _logger.LogWarning(
                CacheVersionLogs.LOG_CACHE_VERSION_INCR_NON_POSITIVE,
                epoch,
                newVersion);

            throw new InvalidOperationException(string.Format(
                CacheVersionLogs.CACHE_VERSION_NOT_ADVANCED,
                cacheGroup,
                cacheScope,
                epoch,
                DEFAULT_VERSION,
                newVersion));
        }

        _logger.LogInformation(
            CacheVersionLogs.LOG_CACHE_VERSION_BUMPED,
            epoch,
            newVersion);

        // Best-effort TTL to avoid accumulating one key per day forever.
        try
        {
            var ttl = await _db.KeyTimeToLiveAsync(key);
            if (ttl is null)
            {
                var ok = await _db.KeyExpireAsync(key, epochLifetime);

                _logger.LogDebug(
                    CacheVersionLogs.LOG_CACHE_VERSION_TTL_APPLIED,
                    epoch,
                    epochLifetime.TotalSeconds,
                    ok);
            }
        }
        catch (Exception ex)
        {
            // Fail-open: TTL is cleanup only.
            _logger.LogWarning(
                ex,
                CacheVersionLogs.LOG_CACHE_VERSION_TTL_SET_FAILED,
                epoch,
                epochLifetime.TotalSeconds,
                newVersion);
        }

        return newVersion;
    }

    /// <summary>
    /// Builds the Redis key for a given cache group, scope, and epoch.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group.</param>
    /// <param name="cacheScope">The optional cache scope.</param>
    /// <param name="epoch">Epoch identifier (e.g., yyyyMMdd).</param>
    /// <returns>Redis key string.</returns>
    private static string GetVersionKey(string cacheGroup, string cacheScope, string epoch)
    {
        var normalizedScope = string.IsNullOrWhiteSpace(cacheScope)
            ? string.Empty
            : string.Format(CACHE_SCOPE_SEGMENT_FORMAT, cacheScope);

        return $"{VERSION_KEY_PREFIX}{cacheGroup}{normalizedScope}:{epoch}";
    }
}
