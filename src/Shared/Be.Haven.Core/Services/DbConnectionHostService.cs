namespace Be.Haven.Core.Services;

/// <summary>
/// Represents a hosted service responsible for initializing and managing the database connection.
/// </summary>
public class DbConnectionHostService : IHostedService
{
    private readonly IConnectionStringProvider _provider;
    private readonly GcpOptions _gcpOptions;
    private readonly ILogger<DbConnectionHostService> _logger;

    /// <summary>
    /// A hosted service responsible for initializing and managing the database connection.
    /// </summary>
    public DbConnectionHostService(
        IConnectionStringProvider provider,
        IOptions<GcpOptions> gcpOptions,
        ILogger<DbConnectionHostService> logger)
    {
        _provider = provider;
        _gcpOptions = gcpOptions.Value;
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
        var connectionName = string.IsNullOrWhiteSpace(_gcpOptions.DatabaseSettings.ConnectionName)
            ? DEFAULT_CONNECTION
            : _gcpOptions.DatabaseSettings.ConnectionName;

        _logger.LogInformation(
            DatabaseConnectionConstants.LOG_DB_CONN_HOST_STARTING,
            connectionName);

        try
        {
            await _provider.RefreshAsync(connectionName, cts.Token);

            // Validate sync path used by AddDbContext
            _ = _provider.GetConnectionString(connectionName);

            _logger.LogInformation(
                DatabaseConnectionConstants.LOG_DB_CONN_HOST_COMPLETED,
                connectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                DatabaseConnectionConstants.LOG_DB_CONN_HOST_FAILED,
                connectionName);

            // Fail-fast: wrap with a clear, actionable message, keep original exception as InnerException.
            throw new InvalidOperationException(
                string.Format(DatabaseConnectionConstants.ERR_DB_CONN_HOST_FAILED, connectionName),
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
