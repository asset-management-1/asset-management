namespace Be.Haven.Shared.Dtos.Options.Gcp;

/// <summary>
/// Configuration options for GCP integration, including project details, logging, database, and secret management.
/// </summary>
public class GcpOptions
{
    /// <summary>
    /// GCP project id; leave empty when running on GKE/Cloud Run to auto-detect.
    /// </summary>
    public string ProjectId { get; set; }

    /// <summary>
    /// GCP project number; used for resource identification.
    /// </summary>
    public string ProjectNumber { get; set; }

    /// <summary>
    /// Logging configuration options.
    /// </summary>
    public LoggingOptions LoggingSettings { get; set; } = new();

    /// <summary>
    /// Database configuration options.
    /// </summary>
    public DatabaseOptions DatabaseSettings { get; set; } = new();

    /// <summary>
    /// Secret Manager configuration options.
    /// </summary>
    public SecretManagerOptions SecretManagerSettings { get; set; } = new();

    /// <summary>
    /// Configuration settings related to OpenTelemetry, which provides
    /// observability solutions, including metrics, tracing, and logging.
    /// </summary>
    public OpenTelemetryOptions OpenTelemetrySettings { get; set; } = new();

    /// <summary>
    /// Google Cloud Storage configuration options.
    /// </summary>
    public GcpStorageOptions GcpStorageSettings { get; set; } = new();
}
