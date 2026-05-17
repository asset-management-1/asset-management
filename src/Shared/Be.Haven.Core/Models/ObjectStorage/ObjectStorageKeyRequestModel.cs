namespace Be.Haven.Core.Models.ObjectStorage;

/// <summary>
/// Represents the data needed to build an owner-scoped object-storage key.
/// </summary>
public class ObjectStorageKeyRequestModel
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
    /// Gets or sets the public identifier of the owner used in the object path.
    /// </summary>
    public Guid OwnerPublicId { get; set; }

    /// <summary>
    /// Gets or sets the object tag that describes the uploaded file role.
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// Gets or sets the original uploaded file name.
    /// </summary>
    public string FileName { get; set; }
}
