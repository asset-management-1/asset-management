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

    /// <inheritdoc />
    public Task<string> PublishAsync(PubsubMessage message) =>
        _publisherClient.PublishAsync(message);
}
