namespace Be.Haven.Shared.Dtos.Requests;

/// <summary>
/// Represents the data required to upload a file to GCP storage.
/// Includes the file itself along with optional metadata such as description,
/// uploader identity, and thumbnail generation settings.
/// </summary>
public class UploadGcpRequest
{
    /// <summary>
    /// Gets or sets the original file to be uploaded.
    /// </summary>
    public IFormFile File { get; set; }

    /// <summary>
    /// Gets or sets an optional description associated with the uploaded file.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the identifier or name of the user who performed the upload.
    /// </summary>
    public string UploadedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the system should generate
    /// a thumbnail version of the uploaded file.
    /// </summary>
    public bool GenerateThumbnail => true;

    /// <summary>
    /// Gets or sets the unique identifier of the resource
    /// </summary>
    public Guid? ResourceId { get; set; }
}
