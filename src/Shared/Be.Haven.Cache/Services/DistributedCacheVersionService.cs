using Be.Haven.Cache.Interfaces;

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
    /// Retrieves the current cache version for a given epoch.
    /// </summary>
    /// <param name="epoch">
    /// The epoch scope for the version counter.
    /// Pass a frozen epoch value (captured once) to avoid mismatch at UTC midnight.
    /// </param>
    /// <returns>
    /// Returns the current version as a long.
    /// If the key does not exist (first use), returns <see cref="DEFAULT_VERSION"/> (1).
    /// </returns>
    public async Task<long> GetAsync(string epoch)
    {
        var key = GetVersionKey(epoch);

        try
        {
            var v = await _db.StringGetAsync(key);

            // First-time access: key doesn't exist -> default version (1).
            if (!v.HasValue)
            {
                _logger.LogDebug(
                    CacheVersionLogs.LOG_CACHE_VERSION_KEY_NOT_FOUND,
                    epoch, key, DEFAULT_VERSION);

                return DEFAULT_VERSION;
            }

            // Defensive parse: avoid unexpected casting issues.
            if (!long.TryParse(v.ToString(), out var parsed))
            {
                _logger.LogWarning(
                    CacheVersionLogs.LOG_CACHE_VERSION_INVALID_VALUE,
                    epoch, key, v.ToString(), DEFAULT_VERSION);

                return DEFAULT_VERSION;
            }

            // Defensive: never allow non-positive versions.
            if (parsed <= 0)
            {
                _logger.LogWarning(
                    CacheVersionLogs.LOG_CACHE_VERSION_NON_POSITIVE_VALUE,
                    epoch, key, parsed, DEFAULT_VERSION);

                return DEFAULT_VERSION;
            }

            _logger.LogDebug(
                CacheVersionLogs.LOG_CACHE_VERSION_RETRIEVED,
                epoch, key, parsed);

            return parsed;
        }
        catch (Exception ex)
        {
            // Fail-open: treat as default version if Redis read fails.
            _logger.LogWarning(
                ex,
                CacheVersionLogs.LOG_CACHE_VERSION_READ_FAILED,
                epoch, key, DEFAULT_VERSION);

            return DEFAULT_VERSION;
        }
    }

    /// <summary>
    /// Atomically bumps (increments) the cache version for a given epoch.
    /// </summary>
    /// <param name="epoch">
    /// The epoch scope for the version counter.
    /// Pass a frozen epoch value (captured once) to avoid mismatch at UTC midnight.
    /// </param>
    /// <returns>
    /// Returns the new incremented version.
    /// </returns>
    public async Task<long> InvalidateAsync(string epoch)
    {
        var key = GetVersionKey(epoch);
        
        // If key doesn't exist, seed it to DEFAULT_VERSION (1) first,
        // so the next INCR becomes 2 (i.e., version actually changes).
        if (!await _db.KeyExistsAsync(key))
        {
            await _db.StringSetAsync(key, DEFAULT_VERSION);
        }

        var newVersion = await _db.StringIncrementAsync(key);

        // Defensive normalization (should not happen, but keep it safe).
        if (newVersion <= 0)
        {
            _logger.LogWarning(
                CacheVersionLogs.LOG_CACHE_VERSION_INCR_NON_POSITIVE,
                epoch, key, newVersion, DEFAULT_VERSION);

            newVersion = DEFAULT_VERSION;

            // Best-effort normalize value back to DEFAULT_VERSION.
            try
            {
                await _db.StringSetAsync(key, newVersion);
            }
            catch
            {
                // fail-open
            }
        }

        _logger.LogInformation(
            CacheVersionLogs.LOG_CACHE_VERSION_BUMPED,
            epoch, key, newVersion);

        // Best-effort TTL to avoid accumulating one key per day forever.
        try
        {
            var ttl = await _db.KeyTimeToLiveAsync(key);
            if (ttl is null)
            {
                var ok = await _db.KeyExpireAsync(key, VERSION_KEY_TTL);

                _logger.LogDebug(
                    CacheVersionLogs.LOG_CACHE_VERSION_TTL_APPLIED,
                    epoch, key, (int)VERSION_KEY_TTL.TotalSeconds, ok);
            }
        }
        catch (Exception ex)
        {
            // Fail-open: TTL is cleanup only.
            _logger.LogWarning(
                ex,
                CacheVersionLogs.LOG_CACHE_VERSION_TTL_SET_FAILED,
                epoch, key, (int)VERSION_KEY_TTL.TotalSeconds, newVersion);
        }

        return newVersion;
    }

    /// <summary>
    /// Builds the Redis key for a given epoch.
    /// </summary>
    /// <param name="epoch">Epoch identifier (e.g., yyyyMMdd).</param>
    /// <returns>Redis key string.</returns>
    private static string GetVersionKey(string epoch) => $"{VERSION_KEY_PREFIX}{epoch}";
}