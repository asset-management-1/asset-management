namespace Be.Haven.Cache.Interfaces;

public interface ICacheVersionService
{
    /// <summary>
    /// Returns the current epoch identifier used to scope the version counter.
    /// Default is UTC day formatted as yyyyMMdd (e.g., "20260105").
    /// </summary>
    /// <returns>A string representing the current UTC date in "yyyyMMdd" format.</returns>
    string GetEpoch();
    
    /// <summary>
    /// Asynchronously retrieves the current cache version for the current epoch.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group being resolved.</param>
    /// <param name="cacheScope">The optional cache scope within the logical group.</param>
    /// <param name="epoch">The epoch window used to partition version counters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the current cache version as a long value.</returns>
    Task<long> GetAsync(string cacheGroup, string cacheScope, string epoch);
    
    /// <summary>
    /// Increments the cache version for the current epoch, effectively invalidating stale cache keys.
    /// Implementations must either return an advanced version or throw; write invalidation must not fail open.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group being invalidated.</param>
    /// <param name="cacheScope">The optional cache scope within the logical group.</param>
    /// <param name="epoch">The epoch window used to partition version counters.</param>
    /// <returns>Returns the new cache version as a long integer.</returns>
    Task<long> InvalidateAsync(string cacheGroup, string cacheScope, string epoch);
}
