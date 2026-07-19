namespace Be.Haven.Core.Helpers;

/// <summary>
/// Creates common Dapper command option presets used across repository-style reads.
/// </summary>
public static class DapperCommandOptionsHelper
{
    /// <summary>
    /// Creates options for a SQL text command with optional cancellation and transaction propagation.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the command.</param>
    /// <param name="transaction">The caller-owned transaction that must execute the command.</param>
    /// <returns>The Dapper command options for text SQL.</returns>
    public static DapperCommandOptions CreateText(
        CancellationToken cancellationToken = default,
        IDbTransaction transaction = null)
    {
        // SQL text callers may optionally borrow an existing transaction without changing command construction.
        return new DapperCommandOptions
        {
            CommandType = CommandType.Text,
            CancellationToken = cancellationToken,
            Transaction = transaction
        };
    }
}
