namespace Be.Haven.Cache.Services;

/// <summary>
/// Provides process-local atomic cache operations for memory mode and tests.
/// </summary>
public sealed class InMemoryAtomicCacheService : IAtomicCacheService
{
    private const int LOCK_STRIPE_COUNT = 64;
    private const int EXPIRATION_SWEEP_INTERVAL = 64;

    private readonly ConcurrentDictionary<string, AtomicCacheEntry> _entries = new();
    private readonly ILogger<InMemoryAtomicCacheService> _logger;
    private readonly SemaphoreSlim[] _locks = Enumerable.Range(0, LOCK_STRIPE_COUNT)
        .Select(_ => new SemaphoreSlim(1, 1))
        .ToArray();
    private long _operationCount;

    /// <summary>
    /// Creates process-local atomic cache storage.
    /// </summary>
    /// <param name="logger">The logger used for atomic cache-operation diagnostics.</param>
    public InMemoryAtomicCacheService(ILogger<InMemoryAtomicCacheService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Stores a value only when the process-local key does not already exist.
    /// </summary>
    /// <param name="key">The cache key to reserve.</param>
    /// <param name="value">The value stored when the reservation succeeds.</param>
    /// <param name="expiration">The fixed lifetime of the reservation.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when the value was stored; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> TrySetIfAbsentAsync(
        string key,
        string value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        SweepExpiredEntriesIfDue();
        var keyLock = GetKeyLock(key);
        await keyLock.WaitAsync(cancellationToken);

        try
        {
            // Expired reservations behave as absent before the conditional write is evaluated.
            RemoveExpiredEntry(key);
            var created = _entries.TryAdd(key, new AtomicCacheEntry(value, DateTime.UtcNow.Add(expiration)));

            _logger.LogInformation(CacheLogs.ATOMIC_CACHE_RESERVATION_COMPLETED, created);
            return created;
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <summary>
    /// Increments a process-local counter atomically while preserving its initial expiration window.
    /// </summary>
    /// <param name="key">The counter key.</param>
    /// <param name="expiration">The fixed lifetime assigned when the counter is first created.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The incremented counter value.</returns>
    public async Task<long> IncrementAsync(
        string key,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        SweepExpiredEntriesIfDue();
        var keyLock = GetKeyLock(key);
        await keyLock.WaitAsync(cancellationToken);

        try
        {
            // A new counter receives one fixed window; later increments preserve that expiration.
            RemoveExpiredEntry(key);

            if (!_entries.TryGetValue(key, out var entry))
            {
                _entries[key] = new AtomicCacheEntry("1", DateTime.UtcNow.Add(expiration));

                _logger.LogInformation(CacheLogs.ATOMIC_CACHE_COUNTER_INCREMENTED, 1);
                return 1;
            }

            var value = long.Parse(entry.Value, CultureInfo.InvariantCulture) + 1;
            _entries[key] = entry with { Value = value.ToString(CultureInfo.InvariantCulture) };

            _logger.LogInformation(CacheLogs.ATOMIC_CACHE_COUNTER_INCREMENTED, value);
            return value;
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <summary>
    /// Removes one process-local atomic cache entry.
    /// </summary>
    /// <param name="key">The key identifying the atomic cache entry.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        SweepExpiredEntriesIfDue();
        var keyLock = GetKeyLock(key);
        await keyLock.WaitAsync(cancellationToken);

        try
        {
            _entries.TryRemove(key, out _);

            _logger.LogInformation(CacheLogs.ATOMIC_CACHE_ENTRY_REMOVED);
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <summary>
    /// Resolves one bounded lock stripe for process-local atomic operations.
    /// </summary>
    /// <param name="key">The cache key requiring serialized access.</param>
    /// <returns>The stable lock stripe for the key.</returns>
    private SemaphoreSlim GetKeyLock(string key)
    {
        var index = (int)((uint)StringComparer.Ordinal.GetHashCode(key) % LOCK_STRIPE_COUNT);
        return _locks[index];
    }

    /// <summary>
    /// Periodically removes expired entries that are no longer requested.
    /// </summary>
    private void SweepExpiredEntriesIfDue()
    {
        if (Interlocked.Increment(ref _operationCount) % EXPIRATION_SWEEP_INTERVAL != 0)
        {
            return;
        }

        var now = DateTime.UtcNow;

        foreach (var entry in _entries)
        {
            if (entry.Value.ExpiresAtUtc <= now)
            {
                _entries.TryRemove(entry.Key, out _);
            }
        }
    }

    /// <summary>
    /// Removes an entry after its fixed expiration window has elapsed.
    /// </summary>
    /// <param name="key">The cache key being inspected.</param>
    private void RemoveExpiredEntry(string key)
    {
        if (_entries.TryGetValue(key, out var entry) && entry.ExpiresAtUtc <= DateTime.UtcNow)
        {
            _entries.TryRemove(key, out _);
        }
    }

    private sealed record AtomicCacheEntry(string Value, DateTime ExpiresAtUtc);
}
