namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Models;

internal sealed class CorePublisherNoAttributeEvent : IBaseMessageEvent
{
    public Guid MessageId { get; } = Guid.NewGuid();

    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    public string TraceId { get; set; }
}
