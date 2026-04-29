namespace Haven.Core.Interfaces.Services.Gcp;

/// <summary>
/// Defines operations for interacting with Google Cloud Storage (GCS), 
/// including uploading, deleting, and generating signed URLs for files.
/// </summary>
public interface IGcpUploadService
{
    /// <summary>
    /// Uploads a file stream to the specified Google Cloud Storage bucket.
    /// </summary>
    /// <param name="request">
    /// Contains details about the file to upload, including the bucket name, 
    /// object name, file stream, and content type.
    /// </param>
    /// <param name="ct">A token to cancel the asynchronous operation if needed.</param>
    /// <returns>
    /// A <see cref="FileUploadResponse"/> containing metadata about the uploaded file, 
    /// such as its object name, content type, and public or signed URL.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the upload fails or the request parameters are invalid.
    /// </exception>
    Task<FileUploadResponse> UploadAsync(FileUploadRequest request, CancellationToken ct);

    /// <summary>
    /// Deletes a specific object (file) from the given Google Cloud Storage bucket.
    /// </summary>
    /// <param name="bucketName">The name of the target GCS bucket.</param>
    /// <param name="objectName">The key or object name representing the file in storage.</param>
    /// <param name="ct">A token to cancel the asynchronous operation if needed.</param>
    /// <returns>
    /// A boolean value indicating whether the deletion was successful.
    /// Returns <c>false</c> if the object does not exist or an error occurs.
    /// </returns>
    Task<bool> DeleteAsync(string bucketName, string objectName, CancellationToken ct);

    /// <summary>
    /// Generates a time-limited signed URL for securely accessing a file in Google Cloud Storage.
    /// </summary>
    /// <param name="bucketName">The name of the GCS bucket containing the file.</param>
    /// <param name="objectName">The key or object name of the file to access.</param>
    /// <param name="expirationTime">The duration after which the signed URL will expire.</param>
    /// <param name="ct">A token to cancel the asynchronous operation if needed.</param>
    /// <returns>
    /// A <see cref="Uri"/> that provides temporary access to the file without requiring authentication.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the signed URL generation fails or input parameters are invalid.
    /// </exception>
    Task<Uri> GetSignedUrl(string bucketName, string objectName, TimeSpan expirationTime, CancellationToken ct);
}
