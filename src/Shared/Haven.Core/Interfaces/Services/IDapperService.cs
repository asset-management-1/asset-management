namespace Haven.Core.Interfaces.Services;

/// <summary>
/// Thin data-access facade on top of Dapper, providing common operations.
/// If provider/connectionString is null or empty in options, the service will use the default configured connection.
/// </summary>
public interface IDapperService
{
    /// <summary>
    /// Executes the specified SQL query asynchronously and retrieves a collection of records mapped to the specified type.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="param">The parameters to pass with the query. This is optional and can be null.</param>
    /// <param name="options">The options to configure the query execution, such as provider, connection string, command type, timeout, and cancellation token. This is optional and can be null.</param>
    /// <typeparam name="T">The type to which the query results should be mapped.</typeparam>
    /// <returns>A task representing the asynchronous operation, containing an enumerable collection record of the specified type.</returns>
    Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object param = null,
        DapperCommandOptions options = null);

    /// <summary>
    /// Executes the specified SQL query asynchronously and retrieves the first record mapped to the specified type,
    /// or a default value if no record is found.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="param">The parameters to pass with the query. This is optional and can be null.</param>
    /// <param name="options">The options to configure the query execution, such as provider, connection string, command type, timeout, and cancellation token. This is optional and can be null.</param>
    /// <typeparam name="T">The type to which the first query result should be mapped.</typeparam>
    /// <returns>A task representing the asynchronous operation, containing the first record of the specified type, or the default value of the type if no record is found.</returns>
    Task<T> QueryFirstOrDefaultAsync<T>(
        string sql,
        object param = null,
        DapperCommandOptions options = null);

    /// <summary>
    /// Executes the specified SQL query asynchronously and retrieves the first record mapped to the specified type.
    /// If the query returns no rows, an InvalidOperationException is thrown.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="param">The parameters to pass with the query. This is optional and can be null.</param>
    /// <param name="options">The options to configure the query execution, such as provider, connection string, command type, timeout, and cancellation token. This is optional and can be null.</param>
    /// <typeparam name="T">The type to which the query result should be mapped.</typeparam>
    /// <returns>A task representing the asynchronous operation, containing the first record of the specified type.</returns>
    Task<T> QueryFirstAsync<T>(
        string sql,
        object param = null,
        DapperCommandOptions options = null);

    /// <summary>
    /// Executes the specified SQL query asynchronously and maps the single result to the specified type.
    /// If no result is found, returns the default value of the type.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="param">The parameters to pass with the query. This is optional and can be null.</param>
    /// <param name="options">The options to configure the query execution, such as provider, connection string, command type, timeout, and cancellation token. This is optional and can be null.</param>
    /// <typeparam name="T">The type to which the query result should be mapped.</typeparam>
    /// <returns>A task representing the asynchronous operation, containing the single mapped result or the default value if no result is found.</returns>
    Task<T> QuerySingleOrDefaultAsync<T>(
        string sql,
        object param = null,
        DapperCommandOptions options = null);

    /// <summary>
    /// Executes the specified SQL query asynchronously and maps the result to a single instance of the specified type.
    /// Throws an exception if the query returns more than one row.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="param">The parameters to pass with the query. This is optional and can be null.</param>
    /// <param name="options">The options to configure the query execution, such as provider, connection string, command type, timeout, and cancellation token. This is optional and can be null.</param>
    /// <typeparam name="T">The type to which the query result should be mapped.</typeparam>
    /// <returns>A task representing the asynchronous operation, containing a single instance of the specified type.</returns>
    Task<T> QuerySingleAsync<T>(
        string sql,
        object param = null,
        DapperCommandOptions options = null);

    /// <summary>
    /// Executes a SQL command asynchronously and returns the number of rows affected.
    /// </summary>
    /// <param name="sql">The SQL command to execute.</param>
    /// <param name="param">The parameters to pass with the command. This is optional and can be null.</param>
    /// <param name="options">The options to configure the command execution, such as provider, connection string, command type, timeout, and cancellation token. This is optional and can be null.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of rows affected by the command.</returns>
    Task<int> ExecuteAsync(
        string sql,
        object param = null,
        DapperCommandOptions options = null);

    /// <summary>
    /// Executes the specified SQL query asynchronously and returns the first column of the first row in the result set. All other columns and rows are ignored.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="param">The parameters to pass with the query. This is optional and can be null.</param>
    /// <param name="options">The options to configure the query execution, such as provider, connection string, command type, timeout, and cancellation token. This is optional and can be null.</param>
    /// <typeparam name="T">The type of the scalar value to be returned.</typeparam>
    /// <returns>A task representing the asynchronous operation, containing the scalar result of the query cast to the specified type.</returns>
    Task<T> ExecuteScalarAsync<T>(
        string sql,
        object param = null,
        DapperCommandOptions options = null);

    /// <summary>
    /// Executes a SQL query asynchronously that returns multiple result sets and maps the results using the provided mapping function.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="map">A function that processes the result from the query and maps it to the specified type.</param>
    /// <param name="param">The parameters to pass with the query. This is optional and can be null.</param>
    /// <param name="options">The options to configure the query execution, such as provider, connection string, command type, timeout, and cancellation token. This is optional and can be null.</param>
    /// <typeparam name="TResult">The type to which the result should be mapped.</typeparam>
    /// <returns>A task representing the asynchronous operation, containing the result of the mapping function applied to the query results.</returns>
    Task<TResult> QueryMultipleAsync<TResult>(
        string sql,
        Func<SqlMapper.GridReader, Task<TResult>> map,
        object param = null,
        DapperCommandOptions options = null);
}
