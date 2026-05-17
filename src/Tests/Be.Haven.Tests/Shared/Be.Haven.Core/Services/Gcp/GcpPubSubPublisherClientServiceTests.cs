using Google.Cloud.PubSub.V1;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services.Gcp;

public sealed class GcpPubSubPublisherClientServiceTests
{
    [Fact]
    public async Task PublishAsync_Should_DelegateToGooglePublisherClient_When_MessageIsProvided()
    {
        // Arrange
        var message = new PubsubMessage
        {
            Data = ByteString.CopyFromUtf8("payload")
        };
        var publisherClient = new Mock<PublisherClient>();
        publisherClient
            .Setup(x => x.PublishAsync(message))
            .ReturnsAsync("message-id");
        var sut = new GcpPubSubPublisherClientService(publisherClient.Object);

        // Act
        var result = await sut.PublishAsync(message);

        // Assert
        result.Should().Be("message-id");
        publisherClient.Verify(x => x.PublishAsync(message), Times.Once);
    }
}
