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

    /// <summary>
    /// Creates SQL text options that borrow the provider transaction owned by an active EF transaction.
    /// </summary>
    /// <param name="dbContext">The EF context participating in the current unit-of-work transaction.</param>
    /// <param name="cancellationToken">The token used to cancel the command.</param>
    /// <returns>The Dapper command options bound to the active provider transaction.</returns>
    public static DapperCommandOptions CreateTransactionalText(
        DbContext dbContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        // Require the caller's unit of work to own the atomic boundary before Dapper borrows its transaction.
        var currentTransaction = dbContext.Database.CurrentTransaction
                                 ?? throw new InvalidOperationException(ACTIVE_DATABASE_TRANSACTION_REQUIRED);

        // Reuse the text-command preset while preserving EF ownership of commit, rollback, and disposal.
        return CreateText(cancellationToken, currentTransaction.GetDbTransaction());
    }
}
