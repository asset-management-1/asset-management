namespace Be.Haven.Cache.Services;

/// <summary>
/// Stores cache-bypass markers in the configured distributed cache so all app instances see the same safety window.
/// </summary>
public sealed class DistributedCacheBypassService : ICacheBypassService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<DistributedCacheBypassService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedCacheBypassService"/> class.
    /// </summary>
    /// <param name="cache">The distributed cache used to store bypass markers.</param>
    /// <param name="logger">The service logger.</param>
    public DistributedCacheBypassService(
        IDistributedCache cache,
        ILogger<DistributedCacheBypassService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Marks one logical cache scope as bypass-only for the current epoch safety window.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group.</param>
    /// <param name="cacheScope">The optional cache scope.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be canceled.</param>
    public async Task MarkBypassAsync(string cacheGroup, string cacheScope, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cacheGroup))
        {
            return;
        }

        var key = BuildKey(cacheGroup, cacheScope);
        try
        {
            // Store a distributed marker so all nodes avoid stale payloads after uncertain invalidation.
            await _cache.SetStringAsync(
                key,
                "1",
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheEpochTtlHelper.GetCurrentEpochLifetime()
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, CacheLogs.CACHE_BYPASS_MARK_FAILED, cacheGroup, cacheScope);
        }
    }

    /// <summary>
    /// Returns whether the logical cache scope should currently bypass cached payloads.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group.</param>
    /// <param name="cacheScope">The optional cache scope.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be canceled.</param>
    /// <returns><c>true</c> when cached payloads should be skipped; otherwise <c>false</c>.</returns>
    public async Task<bool> ShouldBypassAsync(string cacheGroup, string cacheScope, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cacheGroup))
        {
            return false;
        }

        var key = BuildKey(cacheGroup, cacheScope);
        try
        {
            // Missing marker is normal; unsafe cache/version reads are handled by the caller's fallback path.
            var marker = await _cache.GetStringAsync(key, cancellationToken);
            return !string.IsNullOrWhiteSpace(marker);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, CacheLogs.CACHE_BYPASS_READ_FAILED, cacheGroup, cacheScope);
            return false;
        }
    }

    /// <summary>
    /// Builds a stable distributed marker key for one logical cache group and scope.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group.</param>
    /// <param name="cacheScope">The optional cache scope.</param>
    /// <returns>The marker key.</returns>
    private static string BuildKey(string cacheGroup, string cacheScope)
    {
        var scopeSegment = string.IsNullOrWhiteSpace(cacheScope)
            ? string.Empty
            : string.Format(CACHE_SCOPE_SEGMENT_FORMAT, cacheScope);

        return $"{BYPASS_KEY_PREFIX}{cacheGroup}{scopeSegment}";
    }
}
