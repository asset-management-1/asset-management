namespace Be.Haven.Core.Services.Gcp;

/// <summary>
/// Wraps a Google Pub/Sub publisher client behind the shared provider boundary.
/// </summary>
public sealed class GcpPubSubPublisherClientService : IGcpPubSubPublisherClient
{
    private readonly PublisherClient _publisherClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GcpPubSubPublisherClientService"/> class.
    /// </summary>
    /// <param name="publisherClient">The Google Pub/Sub publisher client.</param>
    public GcpPubSubPublisherClientService(PublisherClient publisherClient)
    {
        _publisherClient = publisherClient;
    }

    /// <summary>
    /// Publishes one Pub/Sub message through the wrapped Google client.
    /// </summary>
    /// <param name="message">The Pub/Sub message to publish.</param>
    /// <returns>The published message identifier returned by Google Pub/Sub.</returns>
    public Task<string> PublishAsync(PubsubMessage message)
    {
        // Keep the provider-specific call behind the shared publisher abstraction.
        return _publisherClient.PublishAsync(message);
    }
}
