namespace Be.Haven.Cache.Services;

/// <summary>
/// Keeps a short in-process bypass window for in-memory cache mode.
/// </summary>
public sealed class InMemoryCacheBypassService : ICacheBypassService
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _bypassUntilByKey = new(StringComparer.Ordinal);

    /// <summary>
    /// Marks one logical cache scope as bypass-only for the current epoch safety window.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group.</param>
    /// <param name="cacheScope">The optional cache scope.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be canceled.</param>
    public Task MarkBypassAsync(string cacheGroup, string cacheScope, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cacheGroup))
        {
            return Task.CompletedTask;
        }

        // Store an expiry timestamp instead of only a flag so temporary cache outages self-heal.
        var key = BuildKey(cacheGroup, cacheScope);
        _bypassUntilByKey[key] = DateTimeOffset.UtcNow.Add(CacheEpochTtlHelper.GetCurrentEpochLifetime());

        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns whether the logical cache scope should currently bypass cached payloads.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group.</param>
    /// <param name="cacheScope">The optional cache scope.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be canceled.</param>
    /// <returns><c>true</c> when cached payloads should be skipped; otherwise <c>false</c>.</returns>
    public Task<bool> ShouldBypassAsync(string cacheGroup, string cacheScope, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cacheGroup))
        {
            return Task.FromResult(false);
        }

        var key = BuildKey(cacheGroup, cacheScope);
        if (!_bypassUntilByKey.TryGetValue(key, out var bypassUntil))
        {
            return Task.FromResult(false);
        }

        if (bypassUntil > DateTimeOffset.UtcNow)
        {
            return Task.FromResult(true);
        }

        // Clean expired bypass markers lazily to avoid background timer complexity.
        _bypassUntilByKey.TryRemove(key, out _);
        return Task.FromResult(false);
    }

    /// <summary>
    /// Builds a stable local registry key for one logical cache group and scope.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group.</param>
    /// <param name="cacheScope">The optional cache scope.</param>
    /// <returns>The registry key.</returns>
    private static string BuildKey(string cacheGroup, string cacheScope)
    {
        var scopeSegment = string.IsNullOrWhiteSpace(cacheScope)
            ? string.Empty
            : string.Format(CACHE_SCOPE_SEGMENT_FORMAT, cacheScope);

        return $"{cacheGroup}{scopeSegment}";
    }
}
