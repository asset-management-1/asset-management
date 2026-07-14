namespace Be.Haven.Core.Services.Gcp;

/// <summary>
/// Creates Google Pub/Sub publisher clients using the configured emulator or production endpoint.
/// </summary>
public sealed class GcpPubSubPublisherClientFactoryService : IGcpPubSubPublisherClientFactory
{
    /// <summary>
    /// Creates a Pub/Sub publisher client for the supplied topic.
    /// </summary>
    /// <param name="topicName">The Pub/Sub topic name.</param>
    /// <param name="cancellationToken">The token used to cancel client creation.</param>
    /// <returns>The shared publisher client wrapper.</returns>
    public async Task<IGcpPubSubPublisherClient> CreateAsync(
        TopicName topicName,
        CancellationToken cancellationToken)
    {
        // Let Google client builder choose emulator or production based on local environment configuration.
        var publisherClient = await new PublisherClientBuilder
        {
            TopicName = topicName,
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
        }.BuildAsync(cancellationToken);

        return new GcpPubSubPublisherClientService(publisherClient);
    }
}
