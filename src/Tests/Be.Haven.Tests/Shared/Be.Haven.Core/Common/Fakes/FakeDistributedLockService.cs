namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Fakes;

internal sealed class FakeDistributedLockService : IDistributedLockService
{
    private readonly IAsyncDisposable _handle;

    public FakeDistributedLockService(IAsyncDisposable handle)
    {
        _handle = handle;
    }

    public string LastKey { get; private set; }

    public TimeSpan LastTtl { get; private set; }

    public int AttemptCount { get; private set; }

    public Task<IAsyncDisposable> TryAcquireAsync(
        string key,
        TimeSpan ttl,
        CancellationToken ct)
    {
        AttemptCount++;
        LastKey = key;
        LastTtl = ttl;

        return Task.FromResult(_handle);
    }
}
