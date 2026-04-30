namespace Be.Haven.Cache.Interfaces;

public interface ICacheableMediatrQueryService
{
    /// <summary>
    /// Gets a value indicating whether the cache should be bypassed for this query.
    /// </summary>
    bool BypassCache { get; }

    /// <summary>
    /// Gets the unique cache key associated with this query.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Gets the sliding expiration duration for the cache entry.
    /// Returns null if default or no expiration is applied.
    /// </summary>
    TimeSpan? AbsoluteExpiration { get; }
}