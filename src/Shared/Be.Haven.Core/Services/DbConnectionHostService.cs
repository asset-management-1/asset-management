namespace Be.Haven.Core.Services;

/// <summary>
/// Represents a hosted service responsible for initializing and managing the database connection.
/// </summary>
public class DbConnectionHostService : IHostedService
{
    private readonly IConnectionStringProvider _provider;
    private readonly ILogger<DbConnectionHostService> _logger;

    /// <summary>
    /// A hosted service responsible for initializing and managing the database connection.
    /// </summary>
    public DbConnectionHostService(
        IConnectionStringProvider provider,
        ILogger<DbConnectionHostService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    /// <summary>
    /// Starts the background service asynchronously and initializes the database connection.
    /// </summary>
    /// <param name="cancellationToken">A token used to signal the operation should be canceled.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the database connection initialization fails.</exception>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(DB_CONNECTION_INIT_TIMEOUT_SECONDS));

        _logger.LogInformation(
            DatabaseConnectionConstants.LOG_DB_CONN_HOST_STARTING,
            DEFAULT_CONNECTION);

        try
        {
            await _provider.RefreshAsync(DEFAULT_CONNECTION, cts.Token);

            // Validate sync path used by AddDbContext
            _ = _provider.GetConnectionString(DEFAULT_CONNECTION);

            _logger.LogInformation(
                DatabaseConnectionConstants.LOG_DB_CONN_HOST_COMPLETED,
                DEFAULT_CONNECTION);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                DatabaseConnectionConstants.LOG_DB_CONN_HOST_FAILED,
                DEFAULT_CONNECTION);

            // Fail-fast: wrap with a clear, actionable message, keep original exception as InnerException.
            throw new InvalidOperationException(
                string.Format(DatabaseConnectionConstants.ERR_DB_CONN_HOST_FAILED, DEFAULT_CONNECTION),
                ex);
        }
    }

    /// <summary>
    /// Stops the background service asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token used to signal the operation should be canceled.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}