namespace Be.Haven.Core.Services;

public class CachingService : ICachingService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingService> _logger;
    private readonly IJsonSerializerService _serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingService"/> class.
    /// </summary>
    /// <param name="cache">Distributed cache provider.</param>
    /// <param name="logger">Logger for logging cache operations.</param>
    /// <param name="serializer">Serializer used for converting data to and from JSON.</param>
    public CachingService(
        IDistributedCache cache,
        ILogger<CachingService> logger,
        IJsonSerializerService serializer)
    {
        _cache = cache;
        _logger = logger;
        _serializer = serializer;
    }

    /// <summary>
    /// Retrieves a cached item by key.
    /// </summary>
    /// <typeparam name="T">The expected type of the cached item.</typeparam>
    /// <param name="key">The key identifying the cached item.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>The deserialized cached item if found; otherwise, the default value of <typeparamref name="T"/>.</returns>
    public async Task<T> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        var cachedResponse = await _cache.GetAsync(key, cancellationToken);

        if (cachedResponse is null)
        {
            return default;
        }

        _logger.LogInformation(FETCHED_FROM_CACHE, key);
        return _serializer.Deserialize<T>(Default.GetString(cachedResponse));
    }

    /// <summary>
    /// Removes an item from the cache by key.
    /// </summary>
    /// <param name="key">The key identifying the cached item to remove.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    /// <summary>
    /// Stores data in the distributed cache with the specified sliding expiration.
    /// </summary>
    /// <param name="key">The unique key to identify the cached item.</param>
    /// <param name="data">The data to be stored in the cache.</param>
    /// <param name="slidingExpiration">The time interval after which the cache entry will expire if not accessed.</param>
    /// <param name="cancellationToken">The cancellation token to propagate notification that the operation should be canceled.</param>
    /// <typeparam name="T">The type of the data being cached.</typeparam>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SetAsync<T>(
        string key,
        T data,
        TimeSpan slidingExpiration,
        CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = slidingExpiration
        };

        await SetInternalAsync(key, data, options, cancellationToken);
    }

    /// <summary>
    /// Sets data in the cache with the specified key and an absolute expiration time asynchronously.
    /// </summary>
    /// <param name="key">The cache key used to store the data.</param>
    /// <param name="data">The data to be stored in the cache.</param>
    /// <param name="absoluteExpiration">The time span after which the cache entry will expire.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <typeparam name="T">The type of the data being cached.</typeparam>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SetAbsoluteAsync<T>(
        string key,
        T data,
        TimeSpan absoluteExpiration,
        CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration
        };

        await SetInternalAsync(key, data, options, cancellationToken);
    }

    /// <summary>
    /// Sets data in the cache with the specified key and cache entry options asynchronously.
    /// </summary>
    /// <param name="key">The cache key used to store the data.</param>
    /// <param name="data">The data to be stored in the cache.</param>
    /// <param name="options">The cache entry options specifying expiration policies.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <typeparam name="T">The type of the data being cached.</typeparam>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SetInternalAsync<T>(
        string key,
        T data,
        DistributedCacheEntryOptions options,
        CancellationToken cancellationToken)
    {
        var serializedData = UTF8.GetBytes(_serializer.Serialize(data));

        await _cache.SetAsync(key, serializedData, options, cancellationToken);
        _logger.LogInformation(ADDED_TO_CACHE, key);
    }
}
