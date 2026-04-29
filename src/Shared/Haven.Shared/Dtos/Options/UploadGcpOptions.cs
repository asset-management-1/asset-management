namespace Haven.Shared.Dtos.Options;

/// <summary>
/// Represents the configuration settings required for uploading files to GCP.
/// Includes the base API URL, specific upload endpoint, and authentication key.
/// </summary>
public class UploadGcpOptions
{
    /// <summary>
    /// Gets or sets the base URL of the GCP upload service.
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the endpoint path used to upload files.
    /// </summary>
    public MediaEnpoints EndPoints { get; set; }

    /// <summary>
    /// Gets or sets the API key used to authenticate the upload request.
    /// </summary>
    public string ApiKey { get; set; }
}

/// <summary>
/// Represents the external media-related API endpoints used for
/// uploading and retrieving files.
/// </summary>
public class MediaEnpoints
{
    /// <summary>
    /// Gets or sets the endpoint URL used to upload a file.
    /// This endpoint typically accepts multipart/form-data requests.
    /// </summary>
    public string UploadFile { get; set; }

    /// <summary>
    /// Gets or sets the endpoint URL used to retrieve a file by its identifier.
    /// This is usually a GET request that returns the stored media file or its metadata.
    /// </summary>
    public string GetFileById { get; set; }
}