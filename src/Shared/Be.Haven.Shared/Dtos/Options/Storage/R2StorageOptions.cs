namespace Be.Haven.Shared.Dtos.Options.Storage;

/// <summary>
/// Represents Cloudflare R2 private bucket configuration.
/// </summary>
public class R2StorageOptions
{
    /// <summary>
    /// Gets or sets the S3-compatible endpoint used for upload and delete requests.
    /// </summary>
    [Required, Url]
    public string Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the private bucket name used for uploads.
    /// </summary>
    [Required]
    public string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the R2 access key identifier.
    /// </summary>
    [Required]
    public string AccessKeyId { get; set; }

    /// <summary>
    /// Gets or sets the R2 secret access key.
    /// </summary>
    [Required]
    public string SecretAccessKey { get; set; }

    /// <summary>
    /// Gets or sets the optional public base URL used for non-sensitive public objects.
    /// </summary>
    public string PublicBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the object-key prefix used for KYC files.
    /// </summary>
    public string KycObjectPrefix { get; set; } = DEFAULT_KYC_OBJECT_PREFIX;

    /// <summary>
    /// Gets or sets the object-key prefix used for tenant vehicle images.
    /// </summary>
    public string VehicleObjectPrefix { get; set; } = DEFAULT_VEHICLE_OBJECT_PREFIX;

    /// <summary>
    /// Gets or sets the object-key prefix used for account avatar images.
    /// </summary>
    public string AvatarObjectPrefix { get; set; } = DEFAULT_AVATAR_OBJECT_PREFIX;

    /// <summary>
    /// Gets or sets the object-key prefix used for utility meter evidence.
    /// </summary>
    public string MeterObjectPrefix { get; set; } = DEFAULT_METER_OBJECT_PREFIX;
}
