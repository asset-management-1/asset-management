namespace Be.Haven.Shared.Dtos.Options;

/// <summary>
/// Configuration options for GCP Secret Manager integration.
/// </summary>
public class SecretManagerOptions
{
    /// <summary>
    /// The default version to use when retrieving secrets (e.g., "latest").
    /// </summary>
    public string DefaultSecretVersion { get; set; } = "latest";

    /// <summary>
    /// The GCP region/location for Secret Manager operations.
    /// </summary>
    public string Location { get; set; } = "global";
}