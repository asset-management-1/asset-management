namespace Haven.Cache.Interfaces;

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
    /// <returns>A task that represents the asynchronous operation. The task result contains the current cache version as a long value.</returns>
    Task<long> GetAsync(string epoch);
    
    /// <summary>
    /// Increments the cache version for the current epoch, effectively invalidating stale cache keys.
    /// </summary>
    /// <returns>Returns the new cache version as a long integer.</returns>
    Task<long> InvalidateAsync(string epoch);
}