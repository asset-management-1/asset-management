namespace Be.Haven.Shared.Dtos.Options;

/// <summary>
/// Represents Cloudflare R2 private bucket configuration.
/// </summary>
public class R2StorageOptions
{
    /// <summary>
    /// Default private object-key prefix used for CCCD KYC files.
    /// </summary>
    public const string DefaultKycObjectPrefix = "kyc/cccd";

    /// <summary>
    /// Default object-key prefix used for tenant vehicle images.
    /// </summary>
    public const string DefaultVehicleObjectPrefix = "vehicles";

    /// <summary>
    /// Gets or sets the Cloudflare account identifier.
    /// </summary>
    public string AccountId { get; set; }

    /// <summary>
    /// Gets or sets the optional S3-compatible endpoint override.
    /// </summary>
    public string Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the private bucket name used for uploads.
    /// </summary>
    public string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the R2 access key identifier.
    /// </summary>
    public string AccessKeyId { get; set; }

    /// <summary>
    /// Gets or sets the R2 secret access key.
    /// </summary>
    public string SecretAccessKey { get; set; }

    /// <summary>
    /// Gets or sets the optional public base URL used for non-sensitive public objects.
    /// </summary>
    public string PublicBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the object-key prefix used for KYC files.
    /// </summary>
    public string KycObjectPrefix { get; set; } = DefaultKycObjectPrefix;

    /// <summary>
    /// Gets or sets the object-key prefix used for tenant vehicle images.
    /// </summary>
    public string VehicleObjectPrefix { get; set; } = DefaultVehicleObjectPrefix;
}
