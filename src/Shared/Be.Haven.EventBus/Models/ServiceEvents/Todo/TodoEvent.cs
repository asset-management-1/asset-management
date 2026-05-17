namespace Be.Haven.EventBus.Models.ServiceEvents.Todo;

/// <summary>
/// Represents a To-Do event message published to the EventBus.
/// This class maps to the Haven To-Do Topic and Subscription.
/// </summary>
[Gcp(
    QueueNameConstants.TopicNames.NATION_ADDRESS_TODO_TOPIC,
    QueueNameConstants.SubNames.NATION_ADDRESS_TODO_SUB)]
public class TodoEvent : IBaseMessageEvent
{
    /// <summary>
    /// The content of the To-Do Name to be sent through the EventBus.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The content of the To-Do Value to be sent through the EventBus.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// A unique identifier automatically generated for each message instance.
    /// Used for tracking and correlation purposes.
    /// </summary>
    public Guid MessageId => Guid.NewGuid();

    /// <summary>
    /// The UTC timestamp automatically set to when this event was created.
    /// </summary>
    public DateTime CreatedAt => DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the trace identifier associated with the event,
    /// used to correlate and track event flow across distributed systems.
    /// </summary>
    [JsonProperty("trace_id")]
    public string TraceId { get; set; }
}
