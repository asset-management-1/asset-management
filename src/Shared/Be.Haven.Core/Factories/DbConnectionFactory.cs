namespace Be.Haven.Core.Factories;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _cfg;
    private readonly IGcpSecretService _gcpSecretService;
    private readonly ILogger<DbConnectionFactory> _logger;

    /// <summary>
    /// A factory class responsible for creating and managing database connections with support for
    /// configurable providers such as SQL Server, MySQL, or PostgreSQL. This class integrates with
    /// Google Cloud Platform (GCP) features such as secret management to securely handle database credentials.
    /// </summary>
    public DbConnectionFactory(
        IConfiguration cfg,
        IGcpSecretService gcpSecretService,
        ILogger<DbConnectionFactory> logger)
    {
        _cfg = cfg;
        _gcpSecretService = gcpSecretService;
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously establishes and returns an open database connection using the specified provider and connection string.
    /// </summary>
    /// <param name="provider">The database provider to use for the connection (e.g., SQL Server, MySQL, PostgreSQL).</param>
    /// <param name="connectionString">The connection string to configure the database connection. If null or empty, a default connection string is used.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, containing the opened <see cref="DbConnection"/> object.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the connection string is not configured or is invalid.</exception>
    public async Task<DbConnection> GetOpenConnectionAsync(
        SqlProvider provider,
        string connectionString,
        CancellationToken ct = default)
    {
        var gcpOptions = _cfg.GetSection(GCP_SETTINGS).Get<GcpOptions>() ?? new GcpOptions();
        var connectionName = string.IsNullOrWhiteSpace(gcpOptions.DatabaseSettings.ConnectionName)
            ? DEFAULT_CONNECTION
            : gcpOptions.DatabaseSettings.ConnectionName;

        // 1) Resolve connection string from GCP Secret Manager
        if (gcpOptions.DatabaseSettings.IsUseGcp)
        {
            _logger.LogInformation(
                LOG_USING_GCP_SECRET,
                gcpOptions.DatabaseSettings.SecretId,
                gcpOptions.DatabaseSettings.SecretVersion);

            connectionString = await _gcpSecretService.GetByIdAsync(
                                                          gcpOptions.DatabaseSettings.SecretId,
                                                          gcpOptions.DatabaseSettings.SecretVersion,
                                                          false,
                                                          ct);
        }
        else if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = _cfg.GetConnectionString(connectionName);
        }

        return await CreateAndOpenAsync(provider, connectionString, ct);
    }

    /// <summary>
    /// Asynchronously creates and opens a database connection using the specified provider and connection string.
    /// </summary>
    /// <param name="provider">The database provider to use for the connection (e.g., SQL_Server, MySQL).</param>
    /// <param name="connectionString">The connection string to configure the database connection.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, containing the opened <see cref="DbConnection"/> object.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the connection string is null, empty, or whitespace.</exception>
    /// <exception cref="NotSupportedException"> Thrown if the provided database provider is not supported.</exception>
    private async Task<DbConnection> CreateAndOpenAsync(
        SqlProvider provider,
        string connectionString,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogError(LOG_CONNECTION_STRING_MISSING, provider);
            throw new InvalidOperationException(ERROR_CONNECTION_STRING_NOT_CONFIGURED);
        }

        var conn = Create(provider, connectionString);
        await conn.OpenAsync(ct);

        _logger.LogInformation(LOG_CONNECTION_OPENED, provider);

        return conn;
    }

    /// <summary>
    /// Creates a new database connection for the specified provider using the provided connection string.
    /// </summary>
    /// <param name="provider">The database provider to use for the connection (e.g., SQL Server, MySQL).</param>
    /// <param name="cs">The connection string to configure the database connection.</param>
    /// <returns>A new instance of <see cref="DbConnection"/> corresponding to the specified provider and connection string.</returns>
    /// <exception cref="NotSupportedException">Thrown if the specified database provider is unsupported.</exception>
    private static DbConnection Create(SqlProvider provider, string cs) =>
        provider switch
        {
            SqlProvider.SQL_Server or SqlProvider.MySQL => new SqlConnection(cs),
            SqlProvider.PostgreSQL => new Npgsql.NpgsqlConnection(cs),
            SqlProvider.SQLite => new SqliteConnection(cs),
            _ => throw new NotSupportedException(string.Format(ERROR_UNSUPPORTED_DATABASE_PROVIDER, provider))
        };
}
