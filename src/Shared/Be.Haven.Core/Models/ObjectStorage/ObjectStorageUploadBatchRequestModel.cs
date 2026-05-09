namespace Be.Haven.Core.Models.ObjectStorage;

/// <summary>
/// Represents an owner-scoped batch upload request for object storage.
/// </summary>
public class ObjectStorageUploadBatchRequestModel
{
    /// <summary>
    /// Gets or sets the configured object prefix from application settings.
    /// </summary>
    public string ConfiguredPrefix { get; set; }

    /// <summary>
    /// Gets or sets the default object prefix used when configuration is empty.
    /// </summary>
    public string DefaultPrefix { get; set; }

    /// <summary>
    /// Gets or sets the public identifier of the owner used in every object path.
    /// </summary>
    public Guid OwnerPublicId { get; set; }

    /// <summary>
    /// Gets or sets the file slots to upload.
    /// </summary>
    public IReadOnlyCollection<ObjectStorageUploadFileModel> Files { get; set; } = [];
}
