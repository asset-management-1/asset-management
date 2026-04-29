namespace Haven.Core.Interfaces.Caches;

public interface ICachingService
{
    /// <summary>
    /// Stores an item in the cache with the specified sliding expiration.
    /// </summary>
    /// <typeparam name="T">The type of the data to cache.</typeparam>
    /// <param name="key">The unique key for the cached item.</param>
    /// <param name="data">The data to store in the cache.</param>
    /// <param name="slidingExpiration">The duration to reset the expiration every time the item is accessed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SetAsync<T>(string key, T data, TimeSpan slidingExpiration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an item from the cache by its key.
    /// </summary>
    /// <typeparam name="T">The expected type of the cached data.</typeparam>
    /// <param name="key">The key of the cached item.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The cached item if found; otherwise, the default value of <typeparamref name="T"/>.</returns>
    Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an item from the cache by its key.
    /// </summary>
    /// <param name="key">The key of the cached item to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets data in the cache with the specified key and an absolute expiration time asynchronously.
    /// </summary>
    /// <param name="key">The cache key used to store the data.</param>
    /// <param name="data">The data to be stored in the cache.</param>
    /// <param name="absoluteExpiration">The time span after which the cache entry will expire.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <typeparam name="T">The type of the data being cached.</typeparam>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetAbsoluteAsync<T>(string key, T data, TimeSpan absoluteExpiration, CancellationToken cancellationToken = default);
}
