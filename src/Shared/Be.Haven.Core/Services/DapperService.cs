namespace Be.Haven.Core.Services;

/// <summary>
/// Concrete Dapper-based data access service.
/// - Opens a new DbConnection per operation via the factory (ADO.NET pooling remains effective).
/// - If provider/connectionString are null or empty in options, the factory will fall back to the default configuration.
/// - Disposes connections promptly with "await using".
/// </summary>
public sealed class DapperService : IDapperService
{
    private readonly IDbConnectionFactory _factory;
    private readonly GcpOptions _gcpOptions;

    /// <summary>
    /// Represents a service that provides data access functionality using Dapper.
    /// </summary>
    public DapperService(
        IDbConnectionFactory factory,
        IOptions<GcpOptions> gcpOptions)
    {
        _factory = factory;
        _gcpOptions = gcpOptions.Value;
    }

    /// <summary>
    /// Executes a query asynchronously and maps the result to a sequence of objects.
    /// </summary>
    /// <typeparam name="T">The type of the objects to map the query result to.</typeparam>
    /// <param name="sql">The SQL query string to execute.</param>
    /// <param name="param">An optional set of parameters to bind to the query.</param>
    /// <param name="options">An optional command configuration containing execution settings.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains enumerable of mapped objects.</returns>
    public Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object param = null,
        DapperCommandOptions options = null)
        => WithCommandAsync(sql, param, options,
            (conn, def) => conn.QueryAsync<T>(def));

    /// <summary>
    /// Executes a query asynchronously and retrieves the first record or a default value if no record is found.
    /// </summary>
    /// <typeparam name="T">The type of the object to map the query result to.</typeparam>
    /// <param name="sql">The SQL query string to execute.</param>
    /// <param name="param">An optional set of parameters to bind to the query.</param>
    /// <param name="options">An optional command configuration containing execution settings.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the first mapped object of the specified type or a default value if no record is found.</returns>
    public Task<T> QueryFirstOrDefaultAsync<T>(
        string sql,
        object param = null,
        DapperCommandOptions options = null)
        => WithCommandAsync(sql, param, options,
            (conn, def) => conn.QueryFirstOrDefaultAsync<T>(def));

    /// <summary>
    /// Executes a query asynchronously and returns the first result mapped to an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to map the query result to.</typeparam>
    /// <param name="sql">The SQL query string to execute.</param>
    /// <param name="param">An optional set of parameters to bind to the query.</param>
    /// <param name="options">An optional command configuration containing execution settings.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the first mapped object of the specified type.</returns>
    public Task<T> QueryFirstAsync<T>(
        string sql,
        object param = null,
        DapperCommandOptions options = null)
        => WithCommandAsync(sql, param, options,
            (conn, def) => conn.QueryFirstAsync<T>(def));

    /// <summary>
    /// Executes a SQL query asynchronously and maps the result to a single object or returns the default value if no result is found.
    /// </summary>
    /// <typeparam name="T">The type of the object to map the query result to.</typeparam>
    /// <param name="sql">The SQL query string to execute.</param>
    /// <param name="param">An optional set of parameters to bind to the query.</param>
    /// <param name="options">An optional command configuration containing execution settings.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the mapped object or the default value if no result is found.</returns>
    public Task<T> QuerySingleOrDefaultAsync<T>(
        string sql,
        object param = null,
        DapperCommandOptions options = null)
        => WithCommandAsync(sql, param, options,
            (conn, def) => conn.QuerySingleOrDefaultAsync<T>(def));

    /// <summary>
    /// Executes a SQL query asynchronously and retrieves a single result, mapping it to an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to map the query result to.</typeparam>
    /// <param name="sql">The SQL query string to be executed.</param>
    /// <param name="param">An optional set of parameters to be bound to the query.</param>
    /// <param name="options">An optional command configuration containing settings for execution.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the mapped single object of the specified type.</returns>
    public Task<T> QuerySingleAsync<T>(
        string sql,
        object param = null,
        DapperCommandOptions options = null)
        => WithCommandAsync(sql, param, options,
            (conn, def) => conn.QuerySingleAsync<T>(def));

    /// <summary>
    /// Executes a non-query SQL command asynchronously.
    /// </summary>
    /// <param name="sql">The SQL command to execute.</param>
    /// <param name="param">An optional set of parameters to bind to the command.</param>
    /// <param name="options">An optional command configuration containing execution settings.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the number of rows affected by the command.</returns>
    public Task<int> ExecuteAsync(
        string sql,
        object param = null,
        DapperCommandOptions options = null)
        => WithCommandAsync(sql, param, options,
            (conn, def) => conn.ExecuteAsync(def));

    /// <summary>
    /// Executes a SQL query asynchronously and returns the result as a single scalar value of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value to return.</typeparam>
    /// <param name="sql">The SQL query string to execute.</param>
    /// <param name="param">An optional set of parameters to bind to the query.</param>
    /// <param name="options">An optional command configuration containing execution settings.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the scalar value of the specified type.</returns>
    public Task<T> ExecuteScalarAsync<T>(
        string sql,
        object param = null,
        DapperCommandOptions options = null)
        => WithCommandAsync(sql, param, options,
            (conn, def) => conn.ExecuteScalarAsync<T>(def));

    /// <summary>
    /// Executes a SQL query that returns multiple result sets asynchronously, mapping them using the provided mapping function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result object returned by the mapping function.</typeparam>
    /// <param name="sql">The SQL query string to execute.</param>
    /// <param name="map">A function to map the multiple result sets retrieved by the query.</param>
    /// <param name="param">An optional set of parameters to bind to the query.</param>
    /// <param name="options">An optional command configuration containing execution settings.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the mapped result object.</returns>
    public Task<TResult> QueryMultipleAsync<TResult>(
        string sql,
        Func<SqlMapper.GridReader, Task<TResult>> map,
        object param = null,
        DapperCommandOptions options = null)
        => WithCommandAsync(sql, param, options,
            async (conn, def) =>
            {
                // Execute the SQL and get a GridReader for multiple result sets.
                // 'Await using' ensures the reader is disposed asynchronously after mapping completes.
                await using var grid = await conn.QueryMultipleAsync(def);

                // Delegate mapping to the caller-provided function.
                // The mapper should read all necessary result sets here (e.g., grid.Read<T1>(), grid.Read<T2>(), ...).
                return await map(grid);
            });

    /// <summary>
    /// Executes a database command with the provided SQL, parameters, options, and action, using Dapper.
    /// </summary>
    /// <param name="sql">The SQL query or command to be executed.</param>
    /// <param name="param">The parameters to be passed to the SQL query or command.</param>
    /// <param name="options">Additional options for customizing the command execution, including transaction, timeout, and cancellation settings.</param>
    /// <param name="action">The function to be executed, which takes a database connection and a command definition, and returns a task that produces a result.</param>
    /// <typeparam name="TResult">The type of the result returned by the executed action.</typeparam>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the executed action.</returns>
    private async Task<TResult> WithCommandAsync<TResult>(
        string sql,
        object param,
        DapperCommandOptions options,
        Func<DbConnection, CommandDefinition, Task<TResult>> action)
    {
        // Use defaults if none provided
        options ??= new DapperCommandOptions();

        // Open and auto-dispose connection
        await using var conn = await OpenAsync(options);

        // Build the command definition
        var def = BuildDefinition(sql, param, options);

        // Execute the command and return the results
        return await action(conn, def);
    }

    /// <summary>
    /// Builds a CommandDefinition object based on the provided SQL query, parameters, and execution options.
    /// </summary>
    /// <param name="sql">The SQL query string to be executed.</param>
    /// <param name="param">An optional set of parameters to bind to the SQL query.</param>
    /// <param name="options">An optional command configuration containing execution settings such as command type, timeout, and cancellation token.</param>
    /// <returns>A CommandDefinition object that encapsulates the details of the SQL query, parameters, and execution options.</returns>
    private static CommandDefinition BuildDefinition(
        string sql,
        object param,
        DapperCommandOptions options) =>
        new(
            sql,
            parameters: param,
            transaction: options.Transaction,
            commandTimeout: options.CommandTimeout,
            commandType: options.CommandType, // The default value is a stored procedure
            flags: options.Flags,
            cancellationToken: options.CancellationToken);

    /// <summary>
    /// Opens a database connection asynchronously based on the specified command options.
    /// </summary>
    /// <param name="options">The command configuration containing provider, connection string, and cancellation token settings.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an open database connection.</returns>
    private Task<DbConnection> OpenAsync(DapperCommandOptions options) =>
        _factory.GetOpenConnectionAsync(
            options.Provider ?? _gcpOptions.DatabaseSettings.Provider,
            string.IsNullOrWhiteSpace(options.ConnectionString) ? string.Empty : options.ConnectionString,
            options.CancellationToken);
}
