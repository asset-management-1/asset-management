namespace Be.Haven.Cache.Services;

/// <summary>
/// Provides cross-instance atomic cache operations through the shared Redis connection.
/// </summary>
public sealed class AtomicCacheService : IAtomicCacheService
{
    private readonly IDatabase _database;
    private readonly ILogger<AtomicCacheService> _logger;

    /// <summary>
    /// Creates the Redis atomic cache service.
    /// </summary>
    /// <param name="connectionMultiplexer">The shared Redis connection multiplexer.</param>
    /// <param name="logger">The logger used for atomic cache-operation diagnostics.</param>
    public AtomicCacheService(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<AtomicCacheService> logger)
    {
        _database = connectionMultiplexer.GetDatabase();
        _logger = logger;
    }

    /// <summary>
    /// Stores a value only when the Redis key does not already exist.
    /// </summary>
    /// <param name="key">The Redis key to reserve.</param>
    /// <param name="value">The value stored when the reservation succeeds.</param>
    /// <param name="expiration">The fixed lifetime of the reservation.</param>
    /// <param name="cancellationToken">The token used to cancel the operation before it starts.</param>
    /// <returns><see langword="true"/> when Redis creates the key; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> TrySetIfAbsentAsync(
        string key,
        string value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Redis SET NX with expiry performs the reservation as one atomic command.
        var created = await _database.StringSetAsync(key, value, expiration, When.NotExists);

        _logger.LogInformation(CacheLogs.ATOMIC_CACHE_RESERVATION_COMPLETED, created);
        return created;
    }

    /// <summary>
    /// Increments a Redis counter atomically while preserving its initial expiration window.
    /// </summary>
    /// <param name="key">The Redis counter key.</param>
    /// <param name="expiration">The fixed lifetime assigned when the counter is first created.</param>
    /// <param name="cancellationToken">The token used to cancel the operation before it starts.</param>
    /// <returns>The incremented counter value.</returns>
    public async Task<long> IncrementAsync(
        string key,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // One Lua operation prevents concurrent clients from losing the initial expiry assignment.
        var expirationMilliseconds = Math.Max(1, (long)expiration.TotalMilliseconds);
        var result = await _database.ScriptEvaluateAsync(
            Scripts.INCREMENT_WITH_EXPIRATION,
            [new RedisKey(key)],
            [new RedisValue(expirationMilliseconds.ToString(CultureInfo.InvariantCulture))]);

        var count = (long)result;

        _logger.LogInformation(CacheLogs.ATOMIC_CACHE_COUNTER_INCREMENTED, count);
        return count;
    }

    /// <summary>
    /// Removes one Redis atomic cache entry.
    /// </summary>
    /// <param name="key">The Redis key to remove.</param>
    /// <param name="cancellationToken">The token used to cancel the operation before it starts.</param>
    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _database.KeyDeleteAsync(key);

        _logger.LogInformation(CacheLogs.ATOMIC_CACHE_ENTRY_REMOVED);
    }
}
