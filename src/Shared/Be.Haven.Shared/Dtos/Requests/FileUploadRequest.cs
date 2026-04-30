namespace Be.Haven.Shared.Dtos.Requests;

/// <summary>
/// Represents an upload request to Google Cloud Storage.
/// Includes the file stream and necessary metadata
/// such as bucket name, object key, and MIME type.
/// </summary>
public class FileUploadRequest
{
    /// <summary>
    /// The input data stream containing the file content to upload.
    /// The stream must be readable and positioned at the start
    /// before initiating the upload operation.
    /// </summary>
    public Stream FileStream { get; set; }

    /// <summary>
    /// Name of the Google Cloud Storage bucket into which the file will be uploaded.
    /// This value must conform to GCP bucket naming rules.
    /// </summary>
    public string BucketName { get; set; }

    /// <summary>
    /// Unique key used to identify the object within the bucket.
    /// Commonly represents the file name including folder paths if applicable.
    /// </summary>
    public string ObjectName { get; set; }

    /// <summary>
    /// MIME type describing the format of the uploaded content
    /// such as "image/png" or "application/pdf".
    /// Properly setting this allows correct handling of the file when retrieved.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Public base URL for accessing the uploaded object.
    /// </summary>
    public string BasePublicUrl { get; set; }
}
