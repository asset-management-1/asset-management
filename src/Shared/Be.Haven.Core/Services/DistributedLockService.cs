namespace Be.Haven.Core.Services;

public class DistributedLockService : IDistributedLockService
{
    private readonly IDistributedLockFactory _factory;

    /// <summary>
    /// Provides a service for managing distributed locks, enabling concurrent applications
    /// to coordinate resource access through distributed locking mechanisms.
    /// </summary>
    public DistributedLockService(IConnectionMultiplexer mux) : this(
        RedLockFactory.Create(new List<RedLockMultiplexer>
        {
            new(mux)
        }))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedLockService"/> class using a lock factory.
    /// </summary>
    /// <param name="factory">The distributed lock factory.</param>
    public DistributedLockService(IDistributedLockFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Attempts to acquire a distributed lock asynchronously for a given key and time-to-live (TTL).
    /// </summary>
    /// <param name="key">The unique identifier for the distributed lock.</param>
    /// <param name="ttl">The time-to-live duration for the lock.</param>
    /// <param name="ct">A cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task that returns an <see cref="IAsyncDisposable"/> representing the lock if acquired successfully; null if the lock cannot be acquired.</returns>
    public async Task<IAsyncDisposable> TryAcquireAsync(string key, TimeSpan ttl, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var redLock = await _factory.CreateLockAsync(key, ttl);

        if (!redLock.IsAcquired)
        {
            redLock.Dispose();
            return null;
        }

        return new AsyncDisposeWrapper(redLock);
    }

    /// <summary>
    /// A wrapper class that implements <see cref="IAsyncDisposable"/> to provide asynchronous disposal
    /// for a resource that implements <see cref="IRedLock"/>.
    /// </summary>
    private sealed class AsyncDisposeWrapper : IAsyncDisposable
    {
        private readonly IRedLock _lock;

        /// <summary>
        /// A wrapper class implementing <see cref="IAsyncDisposable"/> that facilitates the
        /// asynchronous disposal of resources that implement <see cref="IRedLock"/>.
        /// </summary>
        public AsyncDisposeWrapper(IRedLock redLock) => _lock = redLock;

        /// <summary>
        /// Performs an asynchronous disposal of the underlying resource.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
        public ValueTask DisposeAsync()
        {
            _lock.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
