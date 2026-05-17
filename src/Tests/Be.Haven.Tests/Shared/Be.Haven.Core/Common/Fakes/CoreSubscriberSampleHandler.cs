namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Fakes;

internal sealed class CoreSubscriberSampleHandler : IEventHandler
{
    public Task HandleAsync(string message, CancellationToken ct) =>
        Task.CompletedTask;
}
