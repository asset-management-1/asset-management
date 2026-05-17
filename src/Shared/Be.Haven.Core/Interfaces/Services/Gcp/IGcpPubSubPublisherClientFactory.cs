namespace Be.Haven.Core.Interfaces.Services.Gcp;

/// <summary>
/// Defines a provider boundary for creating Google Pub/Sub publisher clients.
/// </summary>
public interface IGcpPubSubPublisherClientFactory
{
    /// <summary>
    /// Creates a publisher client for the specified topic.
    /// </summary>
    /// <param name="topicName">The Google Pub/Sub topic name.</param>
    /// <param name="cancellationToken">The token used to cancel client creation.</param>
    /// <returns>A publisher client for the topic.</returns>
    Task<IGcpPubSubPublisherClient> CreateAsync(
        TopicName topicName,
        CancellationToken cancellationToken);
}
