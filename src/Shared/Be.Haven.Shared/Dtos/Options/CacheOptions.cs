namespace Be.Haven.Shared.Dtos.Options;

/// <summary>
/// Represents configuration options for caching behavior.
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Gets or sets the sliding expiration time, in seconds, for cached items.
    /// This determines the duration of inactivity after which a cached item will expire.
    /// Each access to the cached item resets its expiration timer.
    /// </summary>
    public int AbsoluteExpiration { get; set; }

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

    /// <summary>
    /// Gets or sets the cache expiration time in minutes.
    /// </summary>
    public int CacheExpirationInMinutes { get; set; }
}