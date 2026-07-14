namespace Be.Haven.Core.Models.ObjectStorage;

/// <summary>
/// Represents prepared upload content for one owner-scoped object-storage slot.
/// </summary>
public class ObjectStorageUploadContentModel
{
    /// <summary>
    /// Gets or sets the logical slot name used by the caller to retrieve the upload result.
    /// </summary>
    public string SlotName { get; set; }

    /// <summary>
    /// Gets or sets the object-key tag used for this slot.
    /// </summary>
    public string ObjectTag { get; set; }

    /// <summary>
    /// Gets or sets the canonical file name used to derive the object-key extension.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the content type sent to object storage.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the factory that opens the prepared content stream for this upload slot.
    /// </summary>
    public Func<Stream> OpenReadStream { get; set; }
}
