namespace Haven.Core.Interfaces.Providers;

public interface IConnectionStringProvider
{
    /// <summary>
    /// Retrieves the connection string for the specified connection name by looking up the last known good value
    /// or falling back to the configuration source if necessary.
    /// </summary>
    /// <param name="connectionName">The name of the database connection whose connection string is being requested.</param>
    /// <returns>The connection string associated with the specified connection name.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionName"/> is null, empty, or consists only of whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a connection string for the specified <paramref name="connectionName"/>
    /// cannot be resolved from any known source.</exception>
    string GetConnectionString(string connectionName);

    /// <summary>
    /// Refreshes the connection string for the specified connection name by reloading it from its source
    /// and updating the last known good value.
    /// </summary>
    /// <param name="connectionName">The name of the database connection whose connection string needs to be refreshed.</param>
    /// <param name="ct">The <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous operation of refreshing the connection string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionName"/> is null, empty, or consists only of whitespace.</exception>
    Task RefreshAsync(string connectionName, CancellationToken ct = default);
}