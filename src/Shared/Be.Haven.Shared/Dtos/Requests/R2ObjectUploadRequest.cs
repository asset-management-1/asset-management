namespace Be.Haven.Shared.Dtos.Requests;

/// <summary>
/// Represents one private object upload request for Cloudflare R2.
/// </summary>
public class R2ObjectUploadRequest
{
    /// <summary>
    /// Gets or sets the readable content stream positioned at the beginning.
    /// </summary>
    public Stream Content { get; set; }

    /// <summary>
    /// Gets or sets the destination object key inside the private bucket.
    /// </summary>
    public string ObjectKey { get; set; }

    /// <summary>
    /// Gets or sets the uploaded object's content type.
    /// </summary>
    public string ContentType { get; set; }
}
