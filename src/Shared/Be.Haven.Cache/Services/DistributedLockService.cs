namespace Be.Haven.Cache.Services;

/// <summary>
/// Coordinates cross-instance critical sections through the application's shared Redis connection.
/// </summary>
public sealed class DistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<DistributedLockService> _logger;

    /// <summary>
    /// Creates a distributed lock provider backed by the configured Redis database.
    /// </summary>
    /// <param name="connectionMultiplexer">The Redis connection shared with cache infrastructure.</param>
    /// <param name="logger">The logger used for provider diagnostics.</param>
    public DistributedLockService(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<DistributedLockService> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    /// <summary>
    /// Attempts to acquire one auto-renewed Redis lock without waiting on contention.
    /// </summary>
    /// <param name="key">The namespaced resource key.</param>
    /// <param name="leaseDuration">The initial lease duration renewed while the handle remains active.</param>
    /// <param name="cancellationToken">The token used to cancel the Redis operation.</param>
    /// <returns>The acquired handle, or <c>null</c> when the key is already locked.</returns>
    public async Task<IAsyncDisposable> TryAcquireAsync(
        string key,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (leaseDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(leaseDuration),
                leaseDuration,
                ErrorConstants.DistributedLockErrors.LOCK_LEASE_DURATION_INVALID);
        }

        // Step 1: Stop before creating provider state when the caller has already cancelled the operation.
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            // Step 2: Reuse the shared Redis database and let the provider renew ownership while the handle is held.
            var distributedLock = new RedisDistributedLock(
                key,
                _connectionMultiplexer.GetDatabase(),
                options => options.Expiry(leaseDuration));

            // Step 3: A zero-wait attempt separates ordinary contention from a Redis infrastructure failure.
            var handle = await distributedLock.TryAcquireAsync(TimeSpan.Zero, cancellationToken);

            _logger.LogInformation(
                DistributedLockLogs.DISTRIBUTED_LOCK_ACQUISITION_COMPLETED,
                handle is not null,
                leaseDuration);

            return handle;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            // Step 4: Preserve the provider exception in server logs and expose a provider-neutral failure upstream.
            _logger.LogError(
                exception,
                DistributedLockLogs.DISTRIBUTED_LOCK_ACQUISITION_FAILED,
                leaseDuration);

            throw new DistributedLockUnavailableException(exception);
        }
    }
}
