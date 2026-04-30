namespace Be.Haven.Shared.Dtos.Responses;

/// <summary>
/// Represents the response returned after requesting an image by its file identifier.
/// </summary>
public class GetImageByFileIdResponse
{
    /// <summary>
    /// Indicates whether the image retrieval operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the detailed data associated with the retrieved image.
    /// This value is populated only when <see cref="Success"/> is true.
    /// </summary>
    public DetailObject Data { get; set; }

    /// <summary>
    /// Contains the error message describing why the retrieval failed.
    /// This value is populated only when <see cref="Success"/> is false.
    /// </summary>
    public string Error { get; set; }
}

/// <summary>
/// Represents the detailed information associated with an image file
/// retrieved from external storage (e.g., GCP or a document viewer service).
/// </summary>
public class DetailObject
{
    /// <summary>
    /// Gets or sets the fully qualified URL that provides access to the image.
    /// This may be a signed URL or publicly accessible link.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the URL of the generated thumbnail image.
    /// Used primarily for preview display scenarios.
    /// </summary>
    public string Thumbnail { get; set; }

    /// <summary>
    /// Gets or sets the expiration date of the image's signed URL.
    /// After this timestamp, the URL becomes invalid and a new one must be generated.
    /// </summary>
    public DateTime ExpireDate { get; set; }
}
