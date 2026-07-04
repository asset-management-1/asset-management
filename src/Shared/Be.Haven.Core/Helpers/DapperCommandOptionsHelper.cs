namespace Be.Haven.Core.Helpers;

/// <summary>
/// Creates common Dapper command option presets used across repository-style reads.
/// </summary>
public static class DapperCommandOptionsHelper
{
    /// <summary>
    /// Creates options for a SQL text command with the supplied cancellation token.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the command.</param>
    /// <returns>The Dapper command options for text SQL.</returns>
    public static DapperCommandOptions CreateText(CancellationToken cancellationToken = default)
    {
        // Most repository read models use SQL text and only need cancellation propagated.
        return new DapperCommandOptions
        {
            CommandType = CommandType.Text,
            CancellationToken = cancellationToken
        };
    }
}
