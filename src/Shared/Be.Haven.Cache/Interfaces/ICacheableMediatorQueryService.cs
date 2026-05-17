namespace Be.Haven.Cache.Interfaces;

public interface ICacheableMediatorQueryService : IMessage
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
    /// Gets the optional cache scope associated with this query.
    /// This can be a concrete scope string or a well-known sentinel such as the current-user scope.
    /// </summary>
    string CacheScope { get; }

    /// <summary>
    /// Gets the optional cache lifetime requested by the caller.
    /// Returns <c>null</c> when <c>CacheSettings:AbsoluteExpiration</c> should be used.
    /// </summary>
    TimeSpan? AbsoluteExpiration { get; }
}
