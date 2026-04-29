namespace Haven.Core.Interceptors;

public sealed class DbConnectionRefreshInterceptor : DbConnectionInterceptor
{
    private static readonly ConcurrentDictionary<Guid, byte> RefreshedOnce = new();
    private readonly IConnectionStringProvider _provider;
    private readonly ILogger<DbConnectionRefreshInterceptor> _logger;
    private readonly string _connectionName;

    /// <summary>
    /// Intercepts database connection events to handle scenarios such as connection failures,
    /// automatic connection string refresh, and cleanup of tracking information for open and closed connections.
    /// </summary>
    public DbConnectionRefreshInterceptor(
        IConnectionStringProvider provider,
        ILogger<DbConnectionRefreshInterceptor> logger,
        string connectionName)
    {
        _provider = provider;
        _logger = logger;
        _connectionName = connectionName;
    }

    /// <summary>
    /// Handles the event of a database connection failure by attempting to refresh the connection string
    /// and applying it for subsequent retries if needed.
    /// </summary>
    /// <param name="connection">The database connection that encountered the failure.</param>
    /// <param name="eventData">The event data that encapsulates details of the connection failure.</param>
    /// <param name="cancellationToken">An optional cancellation token to observe while waiting for the asynchronous operation to complete.</param>
    /// <returns>A task that represents the asynchronous operation of handling the connection failure.</returns>
    public override async Task ConnectionFailedAsync(
        DbConnection connection,
        ConnectionErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (!RefreshedOnce.TryAdd(eventData.ConnectionId, 0))
            return;

        _logger.LogWarning(
            eventData.Exception,
            DatabaseConnectionConstants.LOG_DB_CONN_FAILED_REFRESHING_ONCE,
            _connectionName,
            eventData.ConnectionId);

        try
        {
            await _provider.RefreshAsync(_connectionName, cancellationToken);

            // Apply refreshed cs for upcoming retries
            connection.ConnectionString = _provider.GetConnectionString(_connectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                DatabaseConnectionConstants.LOG_DB_CONN_REFRESH_FAILED_AFTER_FAILURE,
                _connectionName,
                eventData.ConnectionId);
        }
    }

    /// <summary>
    /// Handles the event of a database connection being successfully opened by removing the connection's
    /// identifier from the list of connections that have had their connection strings refreshed due to a prior failure.
    /// </summary>
    /// <param name="connection">The database connection that has been opened.</param>
    /// <param name="eventData">The event data that encapsulates details of the connection being opened.</param>
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData) => RefreshedOnce.TryRemove(eventData.ConnectionId, out _);

    /// <summary>
    /// Handles the event of a database connection being closed by removing the connection's
    /// identifier from the list of connections that have had their connection strings refreshed due to a prior failure.
    /// </summary>
    /// <param name="connection">The database connection that has been closed.</param>
    /// <param name="eventData">The event data that encapsulates details of the connection being closed.</param>
    public override void ConnectionClosed(DbConnection connection, ConnectionEndEventData eventData) => RefreshedOnce.TryRemove(eventData.ConnectionId, out _);
}