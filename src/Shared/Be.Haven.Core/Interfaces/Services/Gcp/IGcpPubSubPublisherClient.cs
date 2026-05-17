namespace Be.Haven.Core.Interfaces.Services.Gcp;

/// <summary>
/// Defines the provider boundary for publishing a prepared Google Pub/Sub message.
/// </summary>
public interface IGcpPubSubPublisherClient
{
    /// <summary>
    /// Publishes a message to the configured Pub/Sub topic.
    /// </summary>
    /// <param name="message">The prepared Pub/Sub message.</param>
    /// <returns>The provider message identifier.</returns>
    Task<string> PublishAsync(PubsubMessage message);
}
