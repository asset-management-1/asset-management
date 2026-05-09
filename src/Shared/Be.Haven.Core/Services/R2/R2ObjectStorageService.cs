namespace Be.Haven.Core.Services.R2;

/// <summary>
/// Uploads and deletes private objects in Cloudflare R2 through the S3-compatible API.
/// </summary>
public class R2ObjectStorageService : IObjectStorageService
{
    private readonly IThirdPartyApiService _thirdPartyApiService;
    private readonly R2StorageOptions _options;
    private readonly ILogger<R2ObjectStorageService> _logger;

    /// <summary>
    /// Creates the Cloudflare R2 object storage service with shared HTTP execution and upload settings.
    /// </summary>
    /// <param name="thirdPartyApiService">The shared service used to execute third-party HTTP calls.</param>
    /// <param name="options">The configured Cloudflare R2 options.</param>
    /// <param name="logger">The logger used for object-storage operations.</param>
    public R2ObjectStorageService(
        IThirdPartyApiService thirdPartyApiService,
        IOptions<R2StorageOptions> options,
        ILogger<R2ObjectStorageService> logger)
    {
        _thirdPartyApiService = thirdPartyApiService;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Uploads one private object to the configured Cloudflare R2 bucket.
    /// </summary>
    /// <param name="request">The object upload request.</param>
    /// <param name="cancellationToken">The token used to cancel the upload.</param>
    /// <returns>The private object metadata safe to persist.</returns>
    public async Task<ObjectUploadResponseModel> UploadAsync(
        ObjectUploadRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Validate R2 configuration before creating a signed S3-compatible request.
        EnsureConfigured();

        try
        {
            // Copy once so the exact bytes can be hashed and then replayed to R2.
            await using var payload = await CopyToMemoryAsync(request.Content, cancellationToken);
            var checksum = Convert.ToHexString(SHA256.HashData(payload.ToArray())).ToLowerInvariant();
            payload.Position = 0;

            // Build the signed raw-stream PUT request used by Cloudflare R2.
            var contentType = string.IsNullOrWhiteSpace(request.ContentType)
                ? R2_DEFAULT_CONTENT_TYPE
                : request.ContentType.Trim();
            var uploadUri = BuildObjectUri(request.ObjectKey);
            var thirdPartyRequest = new BaseThirdPartyApiRequest
            {
                HttpClientName = UPLOAD_R2_OBJECT,
                Method = PUT,
                Endpoint = uploadUri.ToString(),
                ContentType = contentType,
                ContentStream = payload,
                Headers = R2SignatureHelper.BuildUploadHeaders(
                    _options,
                    uploadUri,
                    request.ObjectKey,
                    contentType,
                    checksum)
            };

            // Execute the upload through the shared third-party HTTP path.
            using var response = await _thirdPartyApiService.HandleDynamicHttpRequest(
                thirdPartyRequest,
                cancellationToken);
            response.EnsureSuccessStatusCode();

            // Return only metadata that callers need to persist, not file content.
            var upload = new ObjectUploadResponseModel
            {
                BucketName = _options.BucketName,
                ObjectKey = request.ObjectKey,
                ContentType = contentType,
                FileSize = payload.Length,
                Checksum = checksum
            };

            _logger.LogInformation(
                CoreLogConstants.R2StorageLogs.R2_UPLOAD_COMPLETED,
                upload.BucketName,
                upload.FileSize);

            return upload;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(
                ex,
                CoreLogConstants.R2StorageLogs.R2_UPLOAD_FAILED,
                _options.BucketName);
            throw new ArgumentException(R2_UPLOAD_FAILED_MESSAGE, ex);
        }
    }

    /// <summary>
    /// Deletes one private object from the configured Cloudflare R2 bucket.
    /// </summary>
    /// <param name="objectKey">The private object key to delete.</param>
    /// <param name="cancellationToken">The token used to cancel the delete call.</param>
    /// <returns><c>true</c> when the object was deleted or already absent; otherwise <c>false</c>.</returns>
    public async Task<bool> DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        // Validate R2 configuration before creating a signed delete request.
        EnsureConfigured();

        // Object key is required because delete calls are used for orphan cleanup.
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            throw new ArgumentException(R2_OBJECT_KEY_REQUIRED_MESSAGE, nameof(objectKey));
        }

        try
        {
            // Build the signed DELETE request for the private object key.
            var deleteUri = BuildObjectUri(objectKey);
            var thirdPartyRequest = new BaseThirdPartyApiRequest
            {
                HttpClientName = UPLOAD_R2_OBJECT,
                Method = DELETE,
                Endpoint = deleteUri.ToString(),
                Headers = R2SignatureHelper.BuildDeleteHeaders(_options, deleteUri, objectKey)
            };

            using var response = await _thirdPartyApiService.HandleDynamicHttpRequest(
                thirdPartyRequest,
                cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // Cleanup is idempotent; already-missing objects are treated as deleted.
                return true;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    CoreLogConstants.R2StorageLogs.R2_DELETE_FAILED,
                    _options.BucketName);

                return false;
            }

            _logger.LogInformation(
                CoreLogConstants.R2StorageLogs.R2_DELETE_COMPLETED,
                _options.BucketName);

            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                CoreLogConstants.R2StorageLogs.R2_DELETE_FAILED,
                _options.BucketName);

            return false;
        }
    }

    /// <summary>
    /// Validates that all required R2 options are present before attempting a signed operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required R2 options are missing.</exception>
    private void EnsureConfigured()
    {
        // R2 signed requests require bucket, credentials, and either explicit endpoint or account id.
        if (string.IsNullOrWhiteSpace(_options.BucketName)
            || string.IsNullOrWhiteSpace(_options.AccessKeyId)
            || string.IsNullOrWhiteSpace(_options.SecretAccessKey)
            || (string.IsNullOrWhiteSpace(_options.Endpoint) && string.IsNullOrWhiteSpace(_options.AccountId)))
        {
            throw new InvalidOperationException(R2_OPTIONS_MISSING_MESSAGE);
        }
    }

    /// <summary>
    /// Copies the source stream into memory so the payload can be hashed and sent with the same bytes.
    /// </summary>
    /// <param name="source">The source content stream.</param>
    /// <param name="cancellationToken">The token used to cancel the copy.</param>
    /// <returns>A memory stream positioned at the beginning.</returns>
    private static async Task<MemoryStream> CopyToMemoryAsync(Stream source, CancellationToken cancellationToken)
    {
        // Use a memory stream because signing requires a checksum before the upload stream is sent.
        var memory = new MemoryStream();
        await source.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        return memory;
    }

    /// <summary>
    /// Builds the S3-compatible object URI for the configured bucket and object key.
    /// </summary>
    /// <param name="objectKey">The private object key.</param>
    /// <returns>The absolute R2 object URI.</returns>
    private Uri BuildObjectUri(string objectKey)
    {
        // Prefer explicit endpoint but fall back to the standard account-based R2 endpoint.
        var endpoint = string.IsNullOrWhiteSpace(_options.Endpoint)
            ? string.Format(R2_DEFAULT_ENDPOINT_FORMAT, _options.AccountId)
            : _options.Endpoint.TrimEnd('/');
        var canonicalPath = R2SignatureHelper.BuildCanonicalPath(objectKey);

        return new Uri($"{endpoint}/{_options.BucketName}{canonicalPath}");
    }
}
