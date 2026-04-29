namespace Haven.Core.Interfaces.Events;

/// <summary>
/// Defines the contract for handling messages received from the EventBus.
/// Implementations should provide the logic for processing incoming messages.
/// </summary>
public interface IEventHandler
{
    Task HandleAsync(string message, CancellationToken ct);
}
