namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Models;

[Gcp("core-topic", "core-subscription")]
internal sealed class CoreSubscriberSampleEvent : IBaseMessageEvent
{
    public Guid MessageId { get; } = Guid.NewGuid();

    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    public string TraceId { get; set; }
}
