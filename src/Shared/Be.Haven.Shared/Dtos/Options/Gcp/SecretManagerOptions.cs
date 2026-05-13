namespace Be.Haven.Shared.Dtos.Options.Gcp;

/// <summary>
/// Configuration options for GCP Secret Manager integration.
/// Secret reads use the <c>latest</c> version when callers do not pass an explicit version.
/// </summary>
public class SecretManagerOptions
{
    /// <summary>
    /// Indicates whether generic secret values should be resolved from GCP Secret Manager.
    /// When <c>false</c>, secret access falls back to local configuration or environment values.
    /// </summary>
    public bool IsUseSecret { get; set; } = false;

    /// <summary>
    /// The GCP region/location for Secret Manager operations.
    /// </summary>
    public string Location { get; set; } = "global";
}
