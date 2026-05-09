namespace Be.Haven.Core.Interfaces.Services.Gcp;

/// <summary>
/// Defines a contract for publishing messages to Google Cloud Pub/Sub.
/// </summary>
public interface IGcpPublisherService
{
    /// <summary>
    /// Publishes a message to a Google Cloud Pub/Sub topic asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the message being published.</typeparam>
    /// <param name="message">The message payload to publish.</param>
    /// <param name="cancellationToken">The token used to cancel the publish operation.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    Task PublishMessageAsync<T>(T message, CancellationToken cancellationToken) where T : IBaseMessageEvent;
}
