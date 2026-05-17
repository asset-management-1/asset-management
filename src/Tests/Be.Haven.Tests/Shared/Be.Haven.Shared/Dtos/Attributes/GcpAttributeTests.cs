namespace Be.Haven.Tests.Shared.Be.Haven.Shared.Dtos.Attributes;

public sealed class GcpAttributeTests
{
    [Fact]
    public void Constructor_Should_SetTopicAndSubscriptionKeys_When_BothArgumentsAreProvided()
    {
        // Act
        var result = new GcpAttribute("topic-key", "subscription-key");

        // Assert
        result.TopicKey.Should().Be("topic-key");
        result.SubscriptionKey.Should().Be("subscription-key");
    }

    [Fact]
    public void Constructor_Should_SetTopicOnly_When_SubscriptionKeyIsNotProvided()
    {
        // Act
        var result = new GcpAttribute("topic-key");

        // Assert
        result.TopicKey.Should().Be("topic-key");
        result.SubscriptionKey.Should().BeNull();
    }
}
