namespace Haven.Core.Interfaces.Quartz;

public interface IDistributedLockService
{
    /// <summary>
    /// Attempts to acquire a distributed lock asynchronously for a specified key with a defined time-to-live (TTL).
    /// </summary>
    /// <param name="key">The unique key identifying the distributed lock to be acquired.</param>
    /// <param name="ttl">The duration for which the lock is valid if acquired.</param>
    /// <param name="ct">A cancellation token to observe for cancellation requests.</param>
    /// <returns>A task that represents an <see cref="IAsyncDisposable"/> instance if the lock is acquired successfully,
    /// or null if the lock acquisition fails.</returns>
    Task<IAsyncDisposable> TryAcquireAsync(string key, TimeSpan ttl, CancellationToken ct);
}
