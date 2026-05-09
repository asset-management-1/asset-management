namespace Be.Haven.Shared.Dtos.Options.Caching;

/// <summary>
/// Represents configuration options for connecting to a Redis server.
/// </summary>
public class RedisOptions
{
    /// <summary>
    /// Gets or sets the hostname or IP address of the Redis server.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// Gets or sets the port number used to connect to the Redis server.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the password used to authenticate with the Redis server.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether SSL is enabled for the Redis connection.
    /// </summary>
    public bool Ssl { get; set; }

    /// <summary>
    /// Gets or sets the database number to be used when connecting to the Redis server.
    /// </summary>
    public int DbNumber { get; set; }

    /// <summary>
    /// The number of times the client will retry to connect if the initial attempt fails.
    /// </summary>
    public int ConnectRetry { get; set; }

    /// <summary>
    /// The maximum time (in milliseconds) to wait when establishing a connection to Redis
    /// before the attempt is aborted.
    /// </summary>
    public int ConnectTimeout { get; set; }

    /// <summary>
    /// The maximum time (in milliseconds) to wait for a synchronous operation 
    /// before throwing an TimeoutException.
    /// </summary>
    public int SyncTimeout { get; set; }
}