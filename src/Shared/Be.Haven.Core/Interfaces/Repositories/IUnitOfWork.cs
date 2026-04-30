namespace Be.Haven.Core.Interfaces.Repositories;

public interface IUnitOfWork
{
    /// <summary>
    /// Gets a value indicating whether there is an active database transaction.
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Asynchronously saves all changes made in the context to the database.
    /// </summary>
    /// <param name="ct">A CancellationToken used to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Asynchronously begins a transaction on the database.
    /// </summary>
    /// <param name="ct">A token to observe while waiting for the task to complete. Default is CancellationToken.None.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the database transaction.</returns>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Asynchronously commits the current database transaction.
    /// </summary>
    /// <param name="ct">A CancellationToken used to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous commit operation.</returns>
    Task CommitTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Asynchronously rolls back the current database transaction.
    /// </summary>
    /// <param name="ct">A CancellationToken used to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous rollback operation.</returns>
    Task RollbackTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Executes the specified action within a database transaction.
    /// If an active transaction already exists, the action runs within that transaction;
    /// otherwise, a new transaction is created for the execution of the action.
    /// </summary>
    /// <param name="action">The action to be executed within the transaction.</param>
    /// <param name="ct">A CancellationToken used to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the executed action.</returns>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously executes a specified action within a transaction. If there is no active transaction,
    /// a new transaction is created, and all changes are committed only upon successful execution of the action.
    /// </summary>
    /// <param name="action">The asynchronous action to execute within the transaction.</param>
    /// <param name="ct">A CancellationToken used to observe while waiting for the task to complete.</param>
    /// <typeparam name="T">The type of the result returned by the executed action.</typeparam>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the executed action.</returns>
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct = default);
}