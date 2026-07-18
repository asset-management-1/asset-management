namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Fakes;

internal sealed class FakeDistributedLockService : IDistributedLockService
{
    private readonly IAsyncDisposable _handle;

    public FakeDistributedLockService(IAsyncDisposable handle)
    {
        _handle = handle;
    }

    public string LastKey { get; private set; }

    public TimeSpan LastLeaseDuration { get; private set; }

    public int AttemptCount { get; private set; }

    public Task<IAsyncDisposable> TryAcquireAsync(
        string key,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken)
    {
        AttemptCount++;
        LastKey = key;
        LastLeaseDuration = leaseDuration;

        return Task.FromResult(_handle);
    }
}
