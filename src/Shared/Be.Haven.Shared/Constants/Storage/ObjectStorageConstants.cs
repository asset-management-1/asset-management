namespace Be.Haven.Shared.Constants.Storage;

/// <summary>
/// Stores shared object-storage key and slot constants used across backend services.
/// </summary>
public static class ObjectStorageConstants
{
    /// <summary>
    /// JPEG file extension commonly emitted by cameras and browsers.
    /// </summary>
    public const string JPG_IMAGE_FILE_EXTENSION = ".jpg";

    /// <summary>
    /// Long-form JPEG file extension.
    /// </summary>
    public const string JPEG_IMAGE_FILE_EXTENSION = ".jpeg";

    /// <summary>
    /// PNG image file extension.
    /// </summary>
    public const string PNG_IMAGE_FILE_EXTENSION = ".png";

    /// <summary>
    /// WebP image file extension.
    /// </summary>
    public const string WEBP_IMAGE_FILE_EXTENSION = ".webp";

    /// <summary>
    /// HEIC image file extension.
    /// </summary>
    public const string HEIC_IMAGE_FILE_EXTENSION = ".heic";

    /// <summary>
    /// HEIF image file extension.
    /// </summary>
    public const string HEIF_IMAGE_FILE_EXTENSION = ".heif";

    /// <summary>
    /// AVIF image file extension.
    /// </summary>
    public const string AVIF_IMAGE_FILE_EXTENSION = ".avif";

    /// <summary>
    /// PDF document file extension.
    /// </summary>
    public const string PDF_FILE_EXTENSION = ".pdf";

    /// <summary>
    /// JPEG image media type.
    /// </summary>
    public const string JPEG_IMAGE_CONTENT_TYPE = "image/jpeg";

    /// <summary>
    /// PNG image media type.
    /// </summary>
    public const string PNG_IMAGE_CONTENT_TYPE = "image/png";

    /// <summary>
    /// WebP image media type.
    /// </summary>
    public const string WEBP_IMAGE_CONTENT_TYPE = "image/webp";

    /// <summary>
    /// HEIC image media type.
    /// </summary>
    public const string HEIC_IMAGE_CONTENT_TYPE = "image/heic";

    /// <summary>
    /// HEIF image media type.
    /// </summary>
    public const string HEIF_IMAGE_CONTENT_TYPE = "image/heif";

    /// <summary>
    /// AVIF image media type.
    /// </summary>
    public const string AVIF_IMAGE_CONTENT_TYPE = "image/avif";

    /// <summary>
    /// PDF document media type.
    /// </summary>
    public const string PDF_CONTENT_TYPE = "application/pdf";

    /// <summary>
    /// File extensions accepted by public image upload contracts before content decoding.
    /// </summary>
    public static readonly IReadOnlySet<string> SupportedImageFileExtensions = new[]
        {
            JPG_IMAGE_FILE_EXTENSION,
            JPEG_IMAGE_FILE_EXTENSION,
            PNG_IMAGE_FILE_EXTENSION,
            WEBP_IMAGE_FILE_EXTENSION,
            HEIC_IMAGE_FILE_EXTENSION,
            HEIF_IMAGE_FILE_EXTENSION,
            AVIF_IMAGE_FILE_EXTENSION
        }
        .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Media types accepted by public image upload contracts before content decoding.
    /// </summary>
    public static readonly IReadOnlySet<string> SupportedImageContentTypes = new[]
        {
            JPEG_IMAGE_CONTENT_TYPE,
            PNG_IMAGE_CONTENT_TYPE,
            WEBP_IMAGE_CONTENT_TYPE,
            HEIC_IMAGE_CONTENT_TYPE,
            HEIF_IMAGE_CONTENT_TYPE,
            AVIF_IMAGE_CONTENT_TYPE
        }
        .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// File extensions accepted by evidence upload contracts that support images or PDF documents.
    /// </summary>
    public static readonly IReadOnlySet<string> SupportedImageOrPdfFileExtensions = SupportedImageFileExtensions
        .Append(PDF_FILE_EXTENSION)
        .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Media types accepted by evidence upload contracts that support images or PDF documents.
    /// </summary>
    public static readonly IReadOnlySet<string> SupportedImageOrPdfContentTypes = SupportedImageContentTypes
        .Append(PDF_CONTENT_TYPE)
        .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Canonical separator used by provider-neutral object keys and public object URLs.
    /// </summary>
    public const char OBJECT_KEY_SEPARATOR = '/';

    /// <summary>
    /// Maximum permitted size for one original image submitted by a client.
    /// </summary>
    public const long MAX_IMAGE_UPLOAD_BYTES = 10L * 1024 * 1024;

    /// <summary>
    /// Maximum multipart request size for a vehicle form containing four image slots.
    /// </summary>
    public const long MAX_VEHICLE_MULTIPART_BODY_BYTES = 45L * 1024 * 1024;

    /// <summary>
    /// Maximum number of evidence images attached to either electricity or water for one meter period.
    /// </summary>
    public const int MAX_METER_EVIDENCE_IMAGE_COUNT = 5;

    /// <summary>
    /// Maximum multipart body size for ten original meter evidence images plus meter form fields.
    /// </summary>
    public const long MAX_METER_MULTIPART_BODY_BYTES = 105L * 1024 * 1024;

    /// <summary>
    /// Preferred target size for an explicitly optimized image object.
    /// </summary>
    public const long OPTIMIZED_IMAGE_TARGET_BYTES = 2L * 1024 * 1024;

    /// <summary>
    /// Largest edge retained by the optional JPEG optimization workflow.
    /// </summary>
    public const uint OPTIMIZED_IMAGE_MAX_LONG_EDGE_PIXELS = 2560;

    /// <summary>
    /// Initial JPEG quality used by the optional optimization workflow.
    /// </summary>
    public const uint OPTIMIZED_IMAGE_INITIAL_QUALITY = 85;

    /// <summary>
    /// Lowest JPEG quality allowed by the optional optimization workflow.
    /// </summary>
    public const uint OPTIMIZED_IMAGE_MINIMUM_QUALITY = 70;

    /// <summary>
    /// JPEG quality decrement used by each optional optimization attempt.
    /// </summary>
    public const uint OPTIMIZED_IMAGE_QUALITY_STEP = 5;

    /// <summary>
    /// Canonical content type used for optimized image objects.
    /// </summary>
    public const string OPTIMIZED_IMAGE_CONTENT_TYPE = "image/jpeg";

    /// <summary>
    /// File extension used for every optimized image object.
    /// </summary>
    public const string OPTIMIZED_IMAGE_FILE_EXTENSION = ".jpg";

    /// <summary>
    /// Generic object-key format for owner-scoped uploads.
    /// </summary>
    public const string OWNER_SCOPED_OBJECT_KEY_FORMAT = "{0}/{1}/{2}-{3}-{4}{5}";

    /// <summary>
    /// UTC timestamp format embedded in generated object keys.
    /// </summary>
    public const string OBJECT_KEY_TIMESTAMP_FORMAT = "yyyyMMddHHmmss";

    /// <summary>
    /// Number of random hexadecimal characters appended to collision-sensitive object keys.
    /// </summary>
    public const int OBJECT_KEY_RANDOM_SUFFIX_LENGTH = 8;

    /// <summary>
    /// Maximum time allowed for best-effort object cleanup after a request is cancelled or persistence fails.
    /// </summary>
    public static readonly TimeSpan OBJECT_STORAGE_CLEANUP_TIMEOUT = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Common slot and object-key tag for front-side uploads.
    /// </summary>
    public const string FRONT_OBJECT_SLOT = "front";

    /// <summary>
    /// Common slot and object-key tag for back-side uploads.
    /// </summary>
    public const string BACK_OBJECT_SLOT = "back";

    /// <summary>
    /// Common slot and object-key tag for side uploads.
    /// </summary>
    public const string SIDE_OBJECT_SLOT = "side";

    /// <summary>
    /// Common slot and object-key tag for avatar uploads.
    /// </summary>
    public const string AVATAR_OBJECT_SLOT = "avatar";

    /// <summary>
    /// Default private object-key prefix used for identity-document KYC files.
    /// </summary>
    public const string DEFAULT_KYC_OBJECT_PREFIX = "identities";

    /// <summary>
    /// Default object-key prefix used for tenant vehicle images.
    /// </summary>
    public const string DEFAULT_VEHICLE_OBJECT_PREFIX = "vehicles";

    /// <summary>
    /// Default object-key prefix used for account avatar images.
    /// </summary>
    public const string DEFAULT_AVATAR_OBJECT_PREFIX = "avatars";

    /// <summary>
    /// Default private object-key prefix used for utility meter evidence.
    /// </summary>
    public const string DEFAULT_METER_OBJECT_PREFIX = "meters";

    /// <summary>
    /// Validation message returned when an uploaded image is empty.
    /// </summary>
    public const string IMAGE_FILE_EMPTY_MESSAGE = "Image file must contain content.";

    /// <summary>
    /// Validation message returned when an uploaded image exceeds the permitted size.
    /// </summary>
    public const string IMAGE_FILE_TOO_LARGE_MESSAGE = "Each image file must not exceed 10 MB.";

    /// <summary>
    /// Validation message returned when uploaded bytes cannot be decoded as a supported image.
    /// </summary>
    public const string IMAGE_FILE_INVALID_MESSAGE =
        "Image file must be a valid JPEG, PNG, WebP, HEIC, HEIF, or AVIF image.";

    /// <summary>
    /// Validation message returned when an image extension is outside the upload contract.
    /// </summary>
    public const string IMAGE_FILE_FORMAT_NOT_SUPPORTED_MESSAGE =
        "Image file extension must be JPG, JPEG, PNG, WebP, HEIC, HEIF, or AVIF.";

    /// <summary>
    /// Validation message returned when optional evidence has no content.
    /// </summary>
    public const string EVIDENCE_FILE_EMPTY_MESSAGE = "Evidence file must contain content.";

    /// <summary>
    /// Validation message returned when optional evidence exceeds the upload limit.
    /// </summary>
    public const string EVIDENCE_FILE_TOO_LARGE_MESSAGE = "Each evidence file must not exceed 10 MB.";

    /// <summary>
    /// Validation message returned for unsupported evidence metadata.
    /// </summary>
    public const string EVIDENCE_FILE_FORMAT_NOT_SUPPORTED_MESSAGE =
        "Evidence file must be a supported image or PDF document.";

    /// <summary>
    /// Validation message returned when evidence bytes do not match a supported format.
    /// </summary>
    public const string EVIDENCE_FILE_CONTENT_INVALID_MESSAGE =
        "Evidence file content must be a valid supported image or PDF document.";

}
