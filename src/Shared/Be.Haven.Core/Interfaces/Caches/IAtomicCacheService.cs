namespace Be.Haven.Core.Interfaces.Caches;

/// <summary>
/// Provides atomic operations for short-lived cache coordination state.
/// </summary>
public interface IAtomicCacheService
{
    /// <summary>
    /// Stores a value only when the key does not already exist.
    /// </summary>
    /// <param name="key">The cache key to reserve.</param>
    /// <param name="value">The value stored when the reservation succeeds.</param>
    /// <param name="expiration">The fixed lifetime of the reservation.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when the value was stored; otherwise, <see langword="false"/>.</returns>
    Task<bool> TrySetIfAbsentAsync(
        string key,
        string value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments a numeric value atomically while preserving its initial expiration window.
    /// </summary>
    /// <param name="key">The counter key.</param>
    /// <param name="expiration">The fixed lifetime assigned when the counter is first created.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The incremented counter value.</returns>
    Task<long> IncrementAsync(
        string key,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes one atomic cache entry.
    /// </summary>
    /// <param name="key">The key identifying the atomic cache entry.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
