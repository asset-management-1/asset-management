namespace Be.Haven.Shared.Constants;

/// <summary>
/// Stores shared object-storage key and slot constants used across backend services.
/// </summary>
public static class ObjectStorageConstants
{
    /// <summary>
    /// Generic object-key format for owner-scoped uploads.
    /// </summary>
    public const string OWNER_SCOPED_OBJECT_KEY_FORMAT = "{0}/{1}/{2}{3}";

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
    /// Default private object-key prefix used for CCCD KYC files.
    /// </summary>
    public const string DEFAULT_KYC_OBJECT_PREFIX = "kyc/cccd";

    /// <summary>
    /// Default object-key prefix used for tenant vehicle images.
    /// </summary>
    public const string DEFAULT_VEHICLE_OBJECT_PREFIX = "vehicles";

    /// <summary>
    /// Default object-key prefix used for account avatar images.
    /// </summary>
    public const string DEFAULT_AVATAR_OBJECT_PREFIX = "avatars";

    /// <summary>
    /// Message used when a required object-storage upload file is missing.
    /// </summary>
    public const string OBJECT_STORAGE_REQUIRED_FILE_MISSING_MESSAGE = "Required upload file is missing.";

    /// <summary>
    /// Message used when a batch object-storage upload fails after cleanup has been attempted.
    /// </summary>
    public const string OBJECT_STORAGE_BATCH_UPLOAD_FAILED_MESSAGE = "Object-storage batch upload failed.";
}
