namespace Be.Haven.Cache.Helpers;

/// <summary>
/// Builds deterministic cache payload keys for cacheable mediator queries and invalidation targets.
/// </summary>
public static class CacheKeyHelper
{
    /// <summary>
    /// Gets the parameter hash used by cacheable requests that do not carry business query parameters.
    /// </summary>
    public static string EmptyParameterHash => BuildParameterHash(string.Empty);

    /// <summary>
    /// Builds a payload key for a cacheable mediator query.
    /// </summary>
    /// <param name="request">The cacheable request that owns the logical group and parameters.</param>
    /// <param name="cacheScope">The resolved cache scope segment.</param>
    /// <param name="epoch">The cache version epoch.</param>
    /// <param name="version">The cache version.</param>
    /// <returns>The deterministic cache payload key.</returns>
    public static string BuildKey(
        ICacheableMediatorQueryService request,
        string cacheScope,
        string epoch,
        long version)
    {
        // Scope is a dedicated key segment, so the hash only represents business query parameters.
        var scopeSegment = string.IsNullOrWhiteSpace(cacheScope)
            ? string.Empty
            : string.Format(CACHE_SCOPE_SEGMENT_FORMAT, cacheScope);
        var parameterHash = BuildParameterHash(request);

        return $"{request.CacheKey}{scopeSegment}:e{epoch}:v{version}:{parameterHash}";
    }

    /// <summary>
    /// Builds a no-parameter payload key for an invalidation target.
    /// </summary>
    /// <param name="target">The cache invalidation target.</param>
    /// <param name="epoch">The cache version epoch.</param>
    /// <param name="version">The cache version.</param>
    /// <returns>The deterministic cache payload key.</returns>
    public static string BuildKey(
        CacheInvalidationTargetModel target,
        string epoch,
        long version)
    {
        // Invalidation targets only carry cache group and scope; business parameters belong to query payload keys.
        var scopeSegment = string.IsNullOrWhiteSpace(target.CacheScope)
            ? string.Empty
            : string.Format(CACHE_SCOPE_SEGMENT_FORMAT, target.CacheScope);

        return $"{target.CacheGroup}{scopeSegment}:e{epoch}:v{version}:{EmptyParameterHash}";
    }

    /// <summary>
    /// Builds the stable hash for a cacheable request's business parameters.
    /// </summary>
    /// <param name="request">The cacheable request.</param>
    /// <returns>The stable parameter hash.</returns>
    public static string BuildParameterHash(ICacheableMediatorQueryService request)
    {
        // Exclude cache-control metadata; only true business query parameters belong in the hash.
        var raw = string.Join(
            PIPE_SEPARATOR,
            request.AsDictionary()
                .Where(x => x.Value is not null
                            && x.Key is not CACHEKEY
                            && x.Key is not BYPASSCACHE
                            && x.Key is not ABSOLUTEEXPIRATION
                            && x.Key is not CACHESCOPE)
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => $"{x.Key}={x.Value}"));

        return BuildParameterHash(raw);
    }

    /// <summary>
    /// Builds a stable hash for raw cache parameter text.
    /// </summary>
    /// <param name="rawParameters">The raw stable parameter string.</param>
    /// <returns>The stable parameter hash.</returns>
    private static string BuildParameterHash(string rawParameters)
    {
        // Keep Redis keys short while preserving deterministic key calculation across behaviors.
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawParameters)))
            .ToLowerInvariant()[..32];
    }
}

