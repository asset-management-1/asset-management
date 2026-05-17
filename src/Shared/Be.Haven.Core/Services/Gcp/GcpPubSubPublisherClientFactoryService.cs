namespace Be.Haven.Core.Services.Gcp;

/// <summary>
/// Creates Google Pub/Sub publisher clients using the configured emulator or production endpoint.
/// </summary>
public sealed class GcpPubSubPublisherClientFactoryService : IGcpPubSubPublisherClientFactory
{
    /// <inheritdoc />
    public async Task<IGcpPubSubPublisherClient> CreateAsync(
        TopicName topicName,
        CancellationToken cancellationToken)
    {
        var publisherClient = await new PublisherClientBuilder
        {
            TopicName = topicName,
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
        }.BuildAsync(cancellationToken);

        return new GcpPubSubPublisherClientService(publisherClient);
    }
}
