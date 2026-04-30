namespace Be.Haven.Core.Interfaces.Commands;

/// <summary>
/// Marker interface for commands that do not return a response.
/// </summary>
public interface ICommand : IRequest<Unit>
{
}

/// <summary>
/// Marker interface for commands that do not return any value or response.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}