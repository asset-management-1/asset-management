using Be.Haven.Cache.Interfaces;

namespace Be.Haven.Cache.Services;

public sealed class InMemoryCacheVersionService : ICacheVersionService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<InMemoryCacheVersionService> _logger;

    /// <summary>
    /// Creates a new <see cref="InMemoryCacheVersionService"/>.
    /// </summary>
    /// <param name="cache">
    /// The distributed cache abstraction. In memory mode this is provided by <c>AddDistributedMemoryCache</c>.
    /// </param>
    /// <param name="logger">Logger instance.</param>
    public InMemoryCacheVersionService(
        IDistributedCache cache,
        ILogger<InMemoryCacheVersionService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Returns the current epoch identifier used to scope the version counter.
    /// Default is UTC day formatted as <c>yyyyMMdd</c> (e.g., <c>20260105</c>).
    /// </summary>
    public string GetEpoch() => DateTime.UtcNow.ToString("yyyyMMdd");

    /// <summary>
    /// Retrieves the current cache version for a given epoch.
    /// Returns <see cref="DEFAULT_VERSION"/> (1) if:
    /// - key is missing,
    /// - stored value is invalid,
    /// - or cache read fails.
    /// </summary>
    public async Task<long> GetAsync(string epoch)
    {
        if (string.IsNullOrWhiteSpace(epoch))
            return DEFAULT_VERSION;

        var key = GetVersionKey(epoch);

        try
        {
            var s = await _cache.GetStringAsync(key);

            if (string.IsNullOrWhiteSpace(s))
                return DEFAULT_VERSION;

            if (!long.TryParse(s, out var v) || v <= 0)
                return DEFAULT_VERSION;

            return v;
        }
        catch (Exception ex)
        {
            // Fail-open: treat as default version if cache read fails.
            _logger.LogWarning(ex,
                InMemoryCacheVersionLogs.LOG_IN_MEMORY_CACHE_VERSION_READ_FAILED,
                epoch, key);

            return DEFAULT_VERSION;
        }
    }

    /// <summary>
    /// Bumps (increments) the cache version for a given epoch.
    /// </summary>
    /// <returns>The new version if write succeeds; otherwise <see cref="DEFAULT_VERSION"/>.</returns>
    public async Task<long> InvalidateAsync(string epoch)
    {
        if (string.IsNullOrWhiteSpace(epoch))
            return DEFAULT_VERSION;

        var key = GetVersionKey(epoch);

        for (var attempt = 1; attempt <= MAX_WRITE_CACHED_RETRIES; attempt++)
        {
            try
            {
                // Read current directly to avoid calling GetAsync
                var s = await _cache.GetStringAsync(key);

                var current = long.TryParse(s, out var v) && v > 0
                    ? v
                    : DEFAULT_VERSION;

                var next = current + 1;

                await _cache.SetStringAsync(
                    key,
                    next.ToString(),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = VERSION_KEY_TTL
                    });

                return next;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    InMemoryCacheVersionLogs.LOG_IN_MEMORY_CACHE_VERSION_WRITE_FAILED,
                    attempt, MAX_WRITE_CACHED_RETRIES, epoch, key);
            }
        }

        return DEFAULT_VERSION;
    }

    /// <summary>
    /// Builds the cache key for a given epoch.
    /// </summary>
    private static string GetVersionKey(string epoch) => $"{VERSION_KEY_PREFIX}{epoch}";
}