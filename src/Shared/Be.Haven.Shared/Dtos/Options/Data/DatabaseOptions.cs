namespace Be.Haven.Shared.Dtos.Options.Data;

/// <summary>
/// Defines the configuration options for database settings commonly used in the application.
/// This includes the ability to configure whether GCP is used,
/// as well as managing the associated secret identifier.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Indicates which SQL provider should be used by EF Core and Dapper.
    /// </summary>
    public SqlProvider Provider { get; set; } = SqlProvider.PostgreSql;

    /// <summary>
    /// The configured connection-string name to resolve from the ConnectionStrings section.
    /// </summary>
    public string ConnectionName { get; set; } = DEFAULT_CONNECTION;

    /// <summary>
    /// Indicates whether to use GCP Secret Manager for the database connection string.
    /// </summary>
    public bool IsUseGcp { get; set; }

    /// <summary>
    /// The identifier of the secret in GCP Secret Manager. Database secrets always read the <c>latest</c> version.
    /// </summary>
    public string SecretId { get; set; }
}
