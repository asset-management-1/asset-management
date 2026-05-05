namespace Be.Haven.Shared.Dtos.Options;

/// <summary>
/// Configuration options for GCP Secret Manager integration.
/// </summary>
public class SecretManagerOptions
{
    /// <summary>
    /// Indicates whether generic secret values should be resolved from GCP Secret Manager.
    /// When <c>false</c>, secret access falls back to local configuration or environment values.
    /// </summary>
    public bool IsUseSecret { get; set; } = false;

    /// <summary>
    /// The default version to use when retrieving secrets (e.g., "latest").
    /// </summary>
    public string DefaultSecretVersion { get; set; } = "latest";

    /// <summary>
    /// The GCP region/location for Secret Manager operations.
    /// </summary>
    public string Location { get; set; } = "global";
}
