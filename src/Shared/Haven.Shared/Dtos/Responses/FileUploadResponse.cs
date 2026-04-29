namespace Haven.Shared.Dtos.Responses;

/// <summary>
/// Represents the response returned after successfully uploading
/// a file to Google Cloud Storage. Contains metadata about the
/// stored object along with an optional publicly accessible URL.
/// </summary>
public class FileUploadResponse
{
    /// <summary>
    /// The final name of the stored object in the target bucket.
    /// Can differ from the original uploaded file name depending
    /// on object naming logic or conflict resolution strategy.
    /// </summary>
    public string ObjectName { get; set; }

    /// <summary>
    /// The MIME type associated with the stored content
    /// such as "image/jpeg" or "application/pdf".
    /// Returned directly from Google Cloud Storage metadata.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// A URL that can be used to access the uploaded file directly.
    /// This value is available only if the environment or bucket
    /// policy permits public access. In secure environments,
    /// this property may be null or replaced with a signed URL.
    /// </summary>
    public string Url { get; set; }
}
