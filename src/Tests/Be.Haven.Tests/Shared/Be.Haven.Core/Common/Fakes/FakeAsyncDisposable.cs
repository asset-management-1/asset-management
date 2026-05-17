namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Fakes;

internal sealed class FakeAsyncDisposable : IAsyncDisposable
{
    public bool IsDisposed { get; private set; }

    public ValueTask DisposeAsync()
    {
        IsDisposed = true;

        return ValueTask.CompletedTask;
    }
}
