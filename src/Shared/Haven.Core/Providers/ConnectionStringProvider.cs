namespace Haven.Core.Providers;

public sealed class ConnectionStringProvider : IConnectionStringProvider
{
    private readonly IConfiguration _configuration;
    private readonly IGcpSecretService _gcpSecretService;
    private readonly ILogger<ConnectionStringProvider> _logger;

    // Last-known-good store (per connectionName)
    private readonly ConcurrentDictionary<string, string> _lkg = new(StringComparer.OrdinalIgnoreCase);

    // Single-flight refresh gate (avoid concurrent refresh storms)
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _refreshGates = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Provides methods for retrieving and managing database connection strings.
    /// </summary>
    public ConnectionStringProvider(
        IConfiguration configuration,
        IGcpSecretService gcpSecretService,
        ILogger<ConnectionStringProvider> logger)
    {
        _configuration = configuration;
        _gcpSecretService = gcpSecretService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the connection string for a specified connection name.
    /// </summary>
    /// <param name="connectionName">The name of the connection for which the connection string is required.</param>
    /// <returns>The connection string for the specified connection name.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionName"/> is null, empty, or consists only of whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the connection string for the specified <paramref name="connectionName"/> is not initialized.</exception>
    public string GetConnectionString(string connectionName)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
            throw new ArgumentException(DatabaseConnectionConstants.ERR_CONNECTION_NAME_REQUIRED, nameof(connectionName));

        // 1) Prefer last-known-good
        if (_lkg.TryGetValue(connectionName, out var cs) && !string.IsNullOrWhiteSpace(cs))
            return cs;

        // 2) Fallback to config
        cs = _configuration.GetConnectionString(connectionName);
        
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException(
                string.Format(DatabaseConnectionConstants.ERR_CONNECTION_NOT_INITIALIZED, connectionName));
        
        _lkg[connectionName] = cs;
        return cs;

    }

    /// <summary>
    /// Refreshes the connection string for the specified connection name by reloading it from its source
    /// and updating the last known good value.
    /// </summary>
    /// <param name="connectionName">The name of the connection whose connection string needs to be refreshed.</param>
    /// <param name="ct">The <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous operation of refreshing the connection string.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionName"/> is null, empty, or consists only of whitespace.</exception>
    public async Task RefreshAsync(string connectionName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
            throw new ArgumentException(DatabaseConnectionConstants.ERR_CONNECTION_NAME_REQUIRED, nameof(connectionName));

        var gate = _refreshGates.GetOrAdd(connectionName, _ => new SemaphoreSlim(1, 1));

        await gate.WaitAsync(ct);
        try
        {
            // Re-load from source and update LKG
            var newCs = await LoadConnectionStringAsync(connectionName, ct);

            _lkg[connectionName] = newCs;

            _logger.LogInformation(
                DatabaseConnectionConstants.LOG_CONNECTION_STRING_REFRESHED,
                connectionName);
        }
        finally
        {
            gate.Release();
        }
    }

    /// <summary>
    /// Loads the connection string for a specified connection name, optionally utilizing GCP Secrets if configured.
    /// </summary>
    /// <param name="connectionName">The name of the connection for which the connection string is to be loaded.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The connection string for the specified connection name.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the connection string for the specified <paramref name="connectionName"/> cannot be retrieved
    /// or is null, empty, or consists only of whitespace.
    /// </exception>
    private async Task<string> LoadConnectionStringAsync(string connectionName, CancellationToken ct)
    {
        var opts = _configuration
                   .GetSection(GCP_SETTINGS)
                   .Get<GcpOptions>() ?? new GcpOptions();

        if (opts.DatabaseSettings.IsUseGcp)
        {
            var cs = await _gcpSecretService.GetByIdAsync(
                                                opts.DatabaseSettings.SecretId,
                                                opts.DatabaseSettings.SecretVersion,
                                                false,
                                                ct);

            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException(
                    string.Format(DatabaseConnectionConstants.ERR_MISSING_CONNECTION_STRING, connectionName));

            _logger.LogInformation(
                DatabaseConnectionConstants.LOG_INIT_FROM_GCP,
                connectionName,
                opts.DatabaseSettings.SecretId,
                opts.DatabaseSettings.SecretVersion);

            return cs;
        }

        var cfg = _configuration.GetConnectionString(connectionName);
        if (string.IsNullOrWhiteSpace(cfg))
            throw new InvalidOperationException(
                string.Format(DatabaseConnectionConstants.ERR_MISSING_CONNECTION_STRING, connectionName));

        _logger.LogInformation(DatabaseConnectionConstants.LOG_INIT_FROM_CONFIG, connectionName);
        return cfg;
    }
}