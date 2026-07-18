namespace Be.Haven.Cache.Services;

/// <summary>
/// Provides serialized application-data access through the configured distributed cache.
/// </summary>
public sealed class CachingService : ICachingService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingService> _logger;
    private readonly IJsonSerializerService _serializer;

    /// <summary>
    /// Creates the serialized cache service.
    /// </summary>
    /// <param name="cache">The configured distributed cache provider.</param>
    /// <param name="logger">The logger used for cache-operation diagnostics.</param>
    /// <param name="serializer">The serializer used for cached application values.</param>
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
    /// Retrieves and deserializes a cached application value.
    /// </summary>
    /// <typeparam name="T">The expected cached value type.</typeparam>
    /// <param name="key">The key identifying the cached value.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The cached value when found; otherwise, the default value of <typeparamref name="T"/>.</returns>
    public async Task<T> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        var cachedResponse = await _cache.GetAsync(key, cancellationToken);

        if (cachedResponse is null)
        {
            _logger.LogInformation(CacheLogs.CACHE_VALUE_NOT_FOUND);
            return default;
        }

        // Generic cache logs omit raw keys because authentication keys can contain identifiers.
        _logger.LogInformation(CacheLogs.CACHE_VALUE_FETCHED);
        return _serializer.Deserialize<T>(Encoding.UTF8.GetString(cachedResponse));
    }

    /// <summary>
    /// Removes one serialized application value from the cache.
    /// </summary>
    /// <param name="key">The key identifying the cached value.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);

        _logger.LogInformation(CacheLogs.CACHE_VALUE_REMOVED);
    }

    /// <summary>
    /// Stores an application value with a sliding expiration window.
    /// </summary>
    /// <typeparam name="T">The type of value being cached.</typeparam>
    /// <param name="key">The key identifying the cached value.</param>
    /// <param name="data">The application value to cache.</param>
    /// <param name="slidingExpiration">The lifetime renewed whenever the value is accessed.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    public async Task SetAsync<T>(
        string key,
        T data,
        TimeSpan slidingExpiration,
        CancellationToken cancellationToken = default)
    {
        await SetInternalAsync(
            key,
            data,
            new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration },
            cancellationToken);
    }

    /// <summary>
    /// Stores an application value with an absolute expiration window.
    /// </summary>
    /// <typeparam name="T">The type of value being cached.</typeparam>
    /// <param name="key">The key identifying the cached value.</param>
    /// <param name="data">The application value to cache.</param>
    /// <param name="absoluteExpiration">The fixed lifetime of the value.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    public async Task SetAbsoluteAsync<T>(
        string key,
        T data,
        TimeSpan absoluteExpiration,
        CancellationToken cancellationToken = default)
    {
        await SetInternalAsync(
            key,
            data,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpiration },
            cancellationToken);
    }

    /// <summary>
    /// Serializes and stores one application value with the supplied expiration policy.
    /// </summary>
    /// <typeparam name="T">The type of value being cached.</typeparam>
    /// <param name="key">The key identifying the cached value.</param>
    /// <param name="data">The application value to cache.</param>
    /// <param name="options">The expiration policy applied by the cache provider.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    private async Task SetInternalAsync<T>(
        string key,
        T data,
        DistributedCacheEntryOptions options,
        CancellationToken cancellationToken)
    {
        var serializedData = Encoding.UTF8.GetBytes(_serializer.Serialize(data));

        await _cache.SetAsync(key, serializedData, options, cancellationToken);

        // Generic cache logs confirm the operation without exposing its storage key.
        _logger.LogInformation(CacheLogs.CACHE_VALUE_ADDED);
    }
}
