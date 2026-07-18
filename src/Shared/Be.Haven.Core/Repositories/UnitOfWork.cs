namespace Be.Haven.Core.Repositories;

/// <summary>
/// Coordinates EF Core persistence, audit stamping, and transactional execution for one DbContext.
/// </summary>
/// <typeparam name="TContext">The EF Core DbContext type owned by the current module.</typeparam>
public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _dbContext;
    private readonly IAuthService _authService;
    private readonly ILogger<UnitOfWork<TContext>> _logger;
    private static readonly string ContextName = typeof(TContext).Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork{TContext}"/> class.
    /// </summary>
    /// <param name="dbContext">The EF Core DbContext being coordinated.</param>
    /// <param name="authService">The current authenticated-principal accessor used for audit stamping.</param>
    /// <param name="logger">The unit-of-work logger.</param>
    public UnitOfWork(
        TContext dbContext,
        IAuthService authService,
        ILogger<UnitOfWork<TContext>> logger)
    {
        _dbContext = dbContext;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Indicates whether there is an active database transaction.
    /// </summary>
    public bool HasActiveTransaction => _dbContext.Database.CurrentTransaction is not null;

    /// <summary>
    /// Asynchronously saves all changes made to the context to the database.
    /// </summary>
    /// <param name="ct">A CancellationToken used to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains the number of state entries written to the database.</returns>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var dateTimeUtcNow = DateTime.UtcNow;
        var userPublicId = _authService.UserId();

        foreach (var entry in _dbContext.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = userPublicId;
                    entry.Entity.CreatedAt = dateTimeUtcNow;
                    entry.Entity.UpdatedBy = null;
                    entry.Entity.UpdatedAt = null;
                    entry.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedBy = userPublicId;
                    entry.Entity.UpdatedAt = dateTimeUtcNow;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified; // soft delete
                    entry.Entity.UpdatedBy = userPublicId;
                    entry.Entity.UpdatedAt = dateTimeUtcNow;
                    entry.Entity.IsDeleted = true;
                    break;
            }
        }

        var changes = await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            LOG_SAVED_CHANGES,
            changes,
            typeof(TContext).Name);

        return changes;
    }

    /// <summary>
    /// Asynchronously begins a database transaction.
    /// </summary>
    /// <param name="ct">A CancellationToken used to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains the initiated database transaction.</returns>
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default) =>
        _dbContext.Database.BeginTransactionAsync(ct);

    /// <summary>
    /// Asynchronously commits the current database transaction.
    /// </summary>
    /// <param name="ct">A CancellationToken used to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task CommitTransactionAsync(CancellationToken ct = default) =>
        _dbContext.Database.CommitTransactionAsync(ct);

    /// <summary>
    /// Asynchronously rolls back the active database transaction if one exists.
    /// </summary>
    /// <param name="ct">A CancellationToken used to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RollbackTransactionAsync(CancellationToken ct = default) =>
        _dbContext.Database.RollbackTransactionAsync(ct);

    /// <summary>
    /// Executes the specified action within a database transaction.
    /// If an active transaction already exists, the action runs within that transaction;
    /// otherwise, a new transaction is created and all changes are committed
    /// only upon successful execution of the action.
    /// </summary>
    /// <param name="action">The asynchronous action to be executed within the transaction.</param>
    /// <param name="ct">A CancellationToken used to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken ct = default)
    {
        // Use the provider execution strategy so the transactional block is treated as a retriable unit.
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async execCt =>
        {
            // If an outer scope has already started a transaction,
            // just execute the action within that existing transaction.
            if (HasActiveTransaction)
            {
                await action(execCt);
                return;
            }

            // Create a new transaction owned by this method.
            await using var tx = await BeginTransactionAsync(execCt);
            try
            {
                // Execute the unit of work.
                await action(execCt);

                // Persist all pending changes in a single batch.
                await SaveChangesAsync(execCt);

                // Commit the transaction if everything completed successfully.
                await CommitTransactionAsync(execCt);

                _logger.LogInformation(
                    LOG_TRANSACTIONAL_BLOCK_COMPLETED,
                    ContextName);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    LOG_TRANSACTIONAL_BLOCK_ERROR,
                    ContextName);

                // Roll back the transaction before preserving the original exception contract for global middleware.
                await RollbackTransactionAsync(execCt);

                throw;
            }
        }, ct);
    }

    /// <summary>
    /// Asynchronously executes a specified action within a transaction and returns a result.
    /// If an active transaction already exists, the action runs within that transaction;
    /// otherwise, a new transaction is created and all changes are committed
    /// only upon successful execution of the action.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the executed action.</typeparam>
    /// <param name="action">The asynchronous action to execute within the transaction.</param>
    /// <param name="ct">A CancellationToken used to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the result of the executed action.
    /// </returns>
    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken ct = default)
    {
        // Use the provider execution strategy so the transactional block is treated as a retriable unit.
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async execCt =>
        {
            // If an outer scope has already started a transaction,
            // just execute the action and return its result.
            if (HasActiveTransaction)
            {
                return await action(execCt);
            }

            // Create a new transaction owned by this method.
            await using var tx = await BeginTransactionAsync(execCt);
            try
            {
                // Execute the unit of work and capture the result.
                var result = await action(execCt);

                // Persist all pending changes in a single batch.
                await SaveChangesAsync(execCt);

                // Commit the transaction if everything completed successfully.
                await CommitTransactionAsync(execCt);

                _logger.LogInformation(
                    LOG_TRANSACTIONAL_BLOCK_COMPLETED,
                    ContextName);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    LOG_TRANSACTIONAL_BLOCK_ERROR,
                    ContextName);

                // Roll back the transaction before preserving the original exception contract for global middleware.
                await RollbackTransactionAsync(execCt);

                throw;
            }
        }, ct);
    }
}
