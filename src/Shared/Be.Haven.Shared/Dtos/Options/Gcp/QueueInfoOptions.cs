namespace Be.Haven.Shared.Dtos.Options.Gcp;

/// <summary>
/// Configuration options for managing queue information, including topics and subscriptions.
/// </summary>
public class QueueInfoOptions
{
    /// <summary>
    /// A dictionary that holds information about message topics.
    /// The key represents the topic name, and the value contains its associated configuration details.
    /// </summary>
    public Dictionary<string, string> Topics { get; set; } = new();

    /// <summary>
    /// A dictionary containing subscription configurations.
    /// The key represents the subscription name, and the value represents its related configuration details.
    /// </summary>
    public Dictionary<string, string> Subscriptions { get; set; } = new();
}
