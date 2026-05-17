namespace Be.Haven.Cache.Interfaces;

/// <summary>
/// Defines a server-side policy that resolves cache invalidation targets for a request type.
/// </summary>
/// <typeparam name="TRequest">The request type whose cache impact is being described.</typeparam>
public interface ICacheInvalidationPolicy<in TRequest>
{
    /// <summary>
    /// Resolves the cache targets that should be invalidated after the request succeeds.
    /// </summary>
    /// <param name="request">The successfully handled request.</param>
    /// <returns>The cache targets that should receive a version bump.</returns>
    IReadOnlyCollection<CacheInvalidationTargetModel> GetTargets(TRequest request);
}
