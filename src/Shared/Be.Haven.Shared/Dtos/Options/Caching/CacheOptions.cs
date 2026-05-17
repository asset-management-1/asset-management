namespace Be.Haven.Shared.Dtos.Options.Caching;

/// <summary>
/// Represents configuration options for caching behavior.
/// </summary>
public class CacheOptions
{
    private int _absoluteExpiration = RedisConstants.DEFAULT_ABSOLUTE_EXPIRATION_SECONDS;

    /// <summary>
    /// Gets or sets the absolute expiration time, in seconds, for cache data payloads.
    /// This is the single configured TTL source for cache-aside read data, defaulting to 10 seconds when not configured.
    /// </summary>
    public int AbsoluteExpiration
    {
        get => _absoluteExpiration;
        set => _absoluteExpiration = value > 0 ? value : RedisConstants.DEFAULT_ABSOLUTE_EXPIRATION_SECONDS;
    }

    /// <summary>
    /// Specifies whether the caching mechanism operates in memory.
    /// Determines if the cache stores items in-memory as opposed to other storage mechanisms.
    /// </summary>
    public bool IsMemory { get; set; }

    /// <summary>
    /// Gets or sets the Redis-specific configuration settings for caching.
    /// This includes options related to the usage and behavior of a Redis server as the cache backend.
    /// </summary>
    public RedisOptions RedisSettings { get; set; }
}
