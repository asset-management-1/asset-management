namespace Haven.Core.Interfaces.Services;

/// <summary>
/// Defines the contract for uploading files to Google Cloud Platform (GCP).
/// </summary>
public interface IUploadGcpService
{
    /// <summary>
    /// Uploads one or more files to GCP based on the provided request.
    /// </summary>
    /// <param name="request">
    /// Contains the file(s) and metadata required to perform the upload operation.
    /// </param>
    /// <returns>
    /// A task that resolves to an <see cref="UploadGcpResponse"/> containing
    /// the file identifier, success status, and any associated error details.
    /// </returns>
    Task<UploadGcpResponse> UploadFilesToGcpAsync(UploadGcpRequest request);

    /// <summary>
    /// Retrieves an image from the external storage (e.g., GCP or Document Viewer API)
    /// using the provided file identifier.
    /// </summary>
    /// <param name="fileId">
    /// The unique identifier of the file whose image data should be retrieved.
    /// </param>
    /// <returns>
    /// A task that resolves to a <see cref="GetImageByFileIdResponse"/>,
    /// containing the image URL, thumbnail URL, expiration time, and status information.
    /// </returns>
    Task<GetImageByFileIdResponse> GetImageByFileIdAsync(Guid fileId);
}
