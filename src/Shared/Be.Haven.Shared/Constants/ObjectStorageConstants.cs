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
    /// Message used when a required object-storage upload file is missing.
    /// </summary>
    public const string OBJECT_STORAGE_REQUIRED_FILE_MISSING_MESSAGE = "Required upload file is missing.";
}
