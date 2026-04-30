namespace Be.Haven.Core.Interfaces.Factories;

public interface IDbConnectionFactory
{
    /// <summary>
    /// Asynchronously creates and opens a database connection using the specified provider and connection string.
    /// </summary>
    /// <param name="provider">The database provider to use for the connection (e.g., SQL Server, MySQL, PostgreSQL).</param>
    /// <param name="connectionString">The connection string used to establish the database connection.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the opened <see cref="DbConnection"/> instance.</returns>
    Task<DbConnection> GetOpenConnectionAsync(
        SqlProvider provider,
        string connectionString,
        CancellationToken ct = default);
}