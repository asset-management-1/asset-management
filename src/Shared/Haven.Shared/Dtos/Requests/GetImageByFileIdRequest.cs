namespace Haven.Shared.Dtos.Requests;

/// <summary>
/// Represents a request model used to retrieve an image
/// based on its media identifier.
/// </summary>
public class GetImageByFileIdRequest
{
    /// <summary>
    /// Gets or sets the unique media identifier used to retrieve the image.
    /// This value must match the ID stored in the external storage system.
    /// </summary>
    public string MediaId { get; set; }
}
