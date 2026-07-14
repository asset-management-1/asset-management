using Google.Cloud.PubSub.V1;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services.Gcp;

public sealed class GcpPubSubPublisherClientFactoryServiceTests
{
    [Fact]
    public async Task CreateAsync_Should_ReturnPublisherClientBoundary_When_EmulatorEndpointIsConfigured()
    {
        // Arrange
        var previousEmulatorHost = Environment.GetEnvironmentVariable("PUBSUB_EMULATOR_HOST");
        Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "localhost:1");
        var sut = new GcpPubSubPublisherClientFactoryService();

        try
        {
            await GcpCredentialTestGuard.ExecuteAsyncOrSkip(async () =>
            {
                // Act
                var result = await sut.CreateAsync(
                    new TopicName("project-id", "topic-id"),
                    CancellationToken.None);

                // Assert
                result.Should().BeOfType<GcpPubSubPublisherClientService>();
            });
        }
        finally
        {
            Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", previousEmulatorHost);
        }
    }
}
