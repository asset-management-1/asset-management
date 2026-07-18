

namespace Be.Haven.Core.Interfaces.Concurrency;

/// <summary>
/// Defines provider-neutral distributed lock acquisition for cross-instance critical sections.
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// Attempts to acquire one distributed lock without waiting for a held lock.
    /// </summary>
    /// <param name="key">The stable key identifying the protected resource.</param>
    /// <param name="leaseDuration">The initial lease duration maintained by the provider while the handle is held.</param>
    /// <param name="cancellationToken">The token used to cancel the infrastructure operation.</param>
    /// <returns>The acquired handle, or <c>null</c> when another owner currently holds the lock.</returns>
    Task<IAsyncDisposable> TryAcquireAsync(
        string key,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);
}
