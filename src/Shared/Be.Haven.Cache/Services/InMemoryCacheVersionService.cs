namespace Be.Haven.Cache.Services;

public sealed class InMemoryCacheVersionService : ICacheVersionService
{
    private static readonly ConcurrentDictionary<string, CacheVersionStateModel> VersionsByKey = new(StringComparer.Ordinal);

    /// <summary>
    /// Returns the current epoch identifier used to scope the version counter.
    /// Default is UTC day formatted as <c>yyyyMMdd</c> (e.g., <c>20260105</c>).
    /// </summary>
    public string GetEpoch() => DateTime.UtcNow.ToString("yyyyMMdd");

    /// <summary>
    /// Retrieves the current cache version for a given cache group, scope, and epoch.
    /// Missing keys return <see cref="DEFAULT_VERSION"/>; unsafe cache reads throw so callers can bypass cache.
    /// </summary>
    public Task<long> GetAsync(string cacheGroup, string cacheScope, string epoch)
    {
        if (string.IsNullOrWhiteSpace(cacheGroup) || string.IsNullOrWhiteSpace(epoch))
        {
            throw new ArgumentException(CacheVersionLogs.CACHE_VERSION_INVALIDATION_ARGUMENTS_MISSING);
        }

        var key = GetVersionKey(cacheGroup, cacheScope, epoch);

        if (!VersionsByKey.TryGetValue(key, out var state))
        {
            return Task.FromResult(DEFAULT_VERSION);
        }

        if (state.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            // Expired version state is removed lazily so the next read starts from the default version.
            VersionsByKey.TryRemove(key, out _);
            return Task.FromResult(DEFAULT_VERSION);
        }

        if (state.Version <= 0)
        {
            throw new InvalidOperationException(string.Format(
                CacheVersionLogs.CACHE_VERSION_NOT_ADVANCED,
                cacheGroup,
                cacheScope,
                epoch,
                DEFAULT_VERSION,
                state.Version));
        }

        return Task.FromResult(state.Version);
    }

    /// <summary>
    /// Bumps (increments) the cache version for a given cache group, scope, and epoch.
    /// </summary>
    /// <returns>The advanced version when the write succeeds.</returns>
    public Task<long> InvalidateAsync(string cacheGroup, string cacheScope, string epoch)
    {
        if (string.IsNullOrWhiteSpace(cacheGroup) || string.IsNullOrWhiteSpace(epoch))
        {
            throw new ArgumentException(CacheVersionLogs.CACHE_VERSION_INVALIDATION_ARGUMENTS_MISSING);
        }

        var key = GetVersionKey(cacheGroup, cacheScope, epoch);
        var now = DateTimeOffset.UtcNow;
        var versionKeyTtl = CacheEpochTtlHelper.GetCurrentEpochLifetime();
        var newState = VersionsByKey.AddOrUpdate(
            key,
            _ => new CacheVersionStateModel(DEFAULT_VERSION + 1, now.Add(versionKeyTtl)),
            (_, current) =>
            {
                // Expired counters restart from the default version; active counters advance atomically.
                var currentVersion = current.ExpiresAtUtc <= now
                    ? DEFAULT_VERSION
                    : current.Version;

                return new CacheVersionStateModel(currentVersion + 1, now.Add(versionKeyTtl));
            });

        return Task.FromResult(newState.Version);
    }

    /// <summary>
    /// Builds the cache key for a given cache group, scope, and epoch.
    /// </summary>
    private static string GetVersionKey(string cacheGroup, string cacheScope, string epoch)
    {
        var normalizedScope = string.IsNullOrWhiteSpace(cacheScope)
            ? string.Empty
            : string.Format(CACHE_SCOPE_SEGMENT_FORMAT, cacheScope);

        return $"{VERSION_KEY_PREFIX}{cacheGroup}{normalizedScope}:{epoch}";
    }
}
