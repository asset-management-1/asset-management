namespace Be.Haven.Core.Interfaces.Events;

/// <summary>
/// Defines the base contract for all message events published to or consumed from the EventBus.
/// Provides a unique identifier and creation timestamp for tracking and auditing.
/// </summary>
public interface IBaseMessageEvent
{
    /// <summary>
    /// A unique identifier for the message event.
    /// Implementations should ensure this ID is generated once per event to support tracking and correlation.
    /// </summary>
    Guid MessageId { get; }

    /// <summary>
    /// The UTC timestamp when the event message was created.
    /// Used for logging, auditing, and time-based processing.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// A unique identifier used to trace and correlate events across system components.
    /// </summary>
    public string TraceId { get; set; }
}
