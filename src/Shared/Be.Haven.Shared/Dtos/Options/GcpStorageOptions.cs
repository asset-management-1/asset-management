namespace Be.Haven.Shared.Dtos.Options;

/// <summary>
/// Represents configuration options for accessing Google Cloud Storage resources.
/// Includes settings for constructing public asset URLs and selecting the target bucket.
/// These options are typically bound from application configuration
/// such as appsettings.json or environment variables.
/// </summary>
public class GcpStorageOptions
{
    /// <summary>
    /// The base URL used to generate publicly accessible links
    /// for uploaded objects. For example:
    /// "https://storage.googleapis.com" or a custom CDN endpoint.
    /// This value may be null or omitted in environments that
    /// restrict direct public access such as DEV or UAT.
    /// </summary>
    public string BasePublicUrl { get; set; }

    /// <summary>
    /// Default Google Cloud Storage bucket name used for uploads
    /// if the upload request does not specify a bucket explicitly.
    /// This value must comply with GCP bucket naming conventions.
    /// </summary>
    public string BucketName { get; set; }
}
