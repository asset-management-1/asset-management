namespace Haven.Core.Services.Gcp;

/// <summary>
/// Provides Google Cloud Storage operations such as uploading objects,
/// deleting objects, and generating signed URLs for temporary access.
/// </summary>
public class GcpUploadService : IGcpUploadService
{
    private readonly ILogger<GcpUploadService> _logger;
    private readonly StorageClient _storageClient;

    /// <summary>
    /// Initializes a new instance of <see cref="GcpUploadService"/>.
    /// Dependencies are injected through constructor-based DI.
    /// </summary>
    public GcpUploadService(
        StorageClient storageClient,
        ILogger<GcpUploadService> logger)
    {
        _storageClient = storageClient;
        _logger = logger;
    }

    /// <summary>
    /// Deletes an object from a specified bucket in Google Cloud Storage.
    /// </summary>
    /// <param name="bucketName">The bucket containing the object.</param>
    /// <param name="objectName">The key or name of the object to delete.</param>
    /// <param name="ct">Cancellation token for aborting the operation.</param>
    /// <returns>
    /// A boolean value indicating whether the object was successfully deleted.
    /// Returns <c>false</c> when any exception occurs during the deletion attempt.
    /// </returns>
    public async Task<bool> DeleteAsync(
        string bucketName,
        string objectName,
        CancellationToken ct)
    {
        try
        {
            await _storageClient.DeleteObjectAsync(
                bucketName,
                objectName,
                cancellationToken: ct);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                DELETE_ERROR,
                objectName, bucketName, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Generates a time-limited signed URL for secure access to a stored object.
    /// </summary>
    /// <param name="bucketName">Name of the bucket containing the target file.</param>
    /// <param name="objectName">Key identifying the object in storage.</param>
    /// <param name="expirationTime">Duration until the signed URL expires.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A <see cref="Uri"/> representing the temporary access link.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the URL signing process fails.
    /// </exception>
    public async Task<Uri> GetSignedUrl(
        string bucketName,
        string objectName,
        TimeSpan expirationTime,
        CancellationToken ct)
    {
        try
        {
            var urlSigner = _storageClient.CreateUrlSigner();
            var signed = await urlSigner.SignAsync(
                bucketName,
                objectName,
                expirationTime,
                HttpMethod.Get,
                cancellationToken: ct);

            return new Uri(signed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                SIGNED_URL_ERROR,
                objectName, bucketName, ex.Message);
            throw new ArgumentException(message: SIGNED_URL_FAILED, ex);
        }
    }

    /// <summary>
    /// Uploads a file stream to Google Cloud Storage and returns metadata information.
    /// </summary>
    /// <param name="request">Upload request containing bucket name, object name, content type, and file stream.</param>
    /// <param name="ct">Cancellation token to stop the upload when required.</param>
    /// <returns>
    /// A <see cref="FileUploadResponse"/> containing upload results such as storage object name,
    /// content type, and public link. Public link accessibility depends on object ACL configuration.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the upload process fails.
    /// </exception>
    public async Task<FileUploadResponse> UploadAsync(
        FileUploadRequest request,
        CancellationToken ct)
    {
        try
        {
            var obj = await _storageClient.UploadObjectAsync(
                request.BucketName,
                request.ObjectName,
                request.ContentType,
                request.FileStream,
                cancellationToken: ct);

            return new FileUploadResponse
            {
                ObjectName = obj.Name,
                ContentType = obj.ContentType,
                Url = $"{request.BasePublicUrl}/{obj.Bucket}/{obj.Name}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                UPLOAD_ERROR,
                request.ObjectName, request.BucketName, ex.Message);
            throw new ArgumentException(message: UPLOAD_FAILED, ex);
        }
    }
}