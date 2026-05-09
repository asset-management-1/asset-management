namespace Be.Haven.Core.Models.ObjectStorage;

/// <summary>
/// Represents one file slot in a backend-mediated object-storage upload batch.
/// </summary>
public class ObjectStorageUploadFileModel
{
    /// <summary>
    /// Gets or sets the logical slot name used to read the uploaded object from the batch result.
    /// </summary>
    public string SlotName { get; set; }

    /// <summary>
    /// Gets or sets the uploaded form file for this slot.
    /// </summary>
    public IFormFile File { get; set; }

    /// <summary>
    /// Gets or sets the object-key tag used for this slot.
    /// </summary>
    public string ObjectTag { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this file slot must be present.
    /// </summary>
    public bool IsRequired { get; set; }
}
