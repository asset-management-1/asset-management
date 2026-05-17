namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Models;

[Gcp("core-topic", "core-subscription")]
internal sealed class CorePublisherSampleEvent : IBaseMessageEvent
{
    public Guid MessageId { get; } = Guid.NewGuid();

    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    public string TraceId { get; set; }

    public string Name { get; set; }
}
