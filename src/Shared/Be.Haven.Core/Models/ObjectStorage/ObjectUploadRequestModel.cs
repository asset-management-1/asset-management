namespace Be.Haven.Core.Models.ObjectStorage;

/// <summary>
/// Represents one backend-mediated object upload request.
/// </summary>
public class ObjectUploadRequestModel
{
    /// <summary>
    /// Gets or sets the readable content stream positioned at the beginning.
    /// </summary>
    public Stream Content { get; set; }

    /// <summary>
    /// Gets or sets the destination object key inside the storage bucket.
    /// </summary>
    public string ObjectKey { get; set; }

    /// <summary>
    /// Gets or sets the uploaded object's content type.
    /// </summary>
    public string ContentType { get; set; }
}
