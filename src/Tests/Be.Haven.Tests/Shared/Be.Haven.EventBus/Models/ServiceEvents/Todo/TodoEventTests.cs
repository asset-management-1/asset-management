using Be.Haven.EventBus.Constants;
using Be.Haven.EventBus.Models.ServiceEvents.Todo;

namespace Be.Haven.Tests.Shared.Be.Haven.EventBus.Models.ServiceEvents.Todo;

public sealed class TodoEventTests
{
    [Fact]
    public void TodoEvent_Should_ExposeTrackingMetadata_When_InstanceIsCreated()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var model = new TodoEvent
        {
            Name = "sync-address",
            Value = "district-1",
            TraceId = "trace-123"
        };

        // Act
        var messageId = model.MessageId;
        var createdAt = model.CreatedAt;
        var after = DateTime.UtcNow;

        // Assert
        messageId.Should().NotBeEmpty();
        createdAt.Should().BeOnOrAfter(before);
        createdAt.Should().BeOnOrBefore(after);
        model.TraceId.Should().Be("trace-123");
    }

    [Fact]
    public void TodoEvent_Should_ExposeConfiguredTopicAndSubscription_When_GcpAttributeIsRead()
    {
        // Act
        var result = typeof(TodoEvent)
            .GetCustomAttributes(typeof(GcpAttribute), inherit: false)
            .Cast<GcpAttribute>()
            .Single();

        // Assert
        result.TopicKey.Should().Be(QueueNameConstants.TopicNames.NATION_ADDRESS_TODO_TOPIC);
        result.SubscriptionKey.Should().Be(QueueNameConstants.SubNames.NATION_ADDRESS_TODO_SUB);
    }
}
