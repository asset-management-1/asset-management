namespace Be.Haven.Shared.Dtos.Responses;

/// <summary>
/// Represents private Cloudflare R2 upload metadata safe for persistence.
/// </summary>
public class R2ObjectUploadResponse
{
    /// <summary>
    /// Gets or sets the private bucket name used by the upload.
    /// </summary>
    public string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the object key stored in the private bucket.
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
