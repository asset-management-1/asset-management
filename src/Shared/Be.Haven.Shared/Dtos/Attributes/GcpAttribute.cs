namespace Be.Haven.Shared.Dtos.Attributes;

/// <summary>
/// Custom attribute used to annotate classes that interact with Google Cloud Pub/Sub.
/// It stores configuration information such as Topic ID and Subscription ID
/// for a particular message handler or service.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class GcpAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the Pub/Sub Topic ID associated with the message handler or service.
    /// Used to specify the topic to which messages will be published in Google Cloud Pub/Sub integration.
    /// </summary>
    public string TopicKey { get; set; }

    /// <summary>
    /// Gets or sets the Pub/Sub Subscription ID associated with the message handler or service.
    /// Used within the Google Cloud Pub/Sub integration to specify the subscription to listen to.
    /// </summary>
    public string SubscriptionKey { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GcpAttribute"/> attribute
    /// with the specified Pub/Sub Topic ID and Subscription ID.
    /// </summary>
    /// <param name="topicKey">The Pub/Sub Topic ID.</param>
    /// <param name="subscriptionKey">The Pub/Sub Subscription ID.</param>
    public GcpAttribute(
        string topicKey,
        string subscriptionKey)
    {
        TopicKey = topicKey;
        SubscriptionKey = subscriptionKey;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GcpAttribute"/> attribute
    /// with the specified Pub/Sub Topic ID.
    /// </summary>
    /// <param name="topicKey">The Pub/Sub Topic ID.</param>
    public GcpAttribute(string topicKey)
    {
        TopicKey = topicKey;
    }
}