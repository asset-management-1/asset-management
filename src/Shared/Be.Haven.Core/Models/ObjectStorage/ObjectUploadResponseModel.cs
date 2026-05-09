namespace Be.Haven.Core.Models.ObjectStorage;

/// <summary>
/// Represents uploaded object metadata safe for persistence.
/// </summary>
public class ObjectUploadResponseModel
{
    /// <summary>
    /// Gets or sets the bucket name used by the upload.
    /// </summary>
    public string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the object key stored in the bucket.
    /// </summary>
    public string ObjectKey { get; set; }

    /// <summary>
    /// Gets or sets the uploaded object's content type.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the uploaded object size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the SHA-256 checksum of the uploaded payload.
    /// </summary>
    public string Checksum { get; set; }
}
