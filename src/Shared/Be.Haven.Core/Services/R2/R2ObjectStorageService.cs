namespace Be.Haven.Core.Services.R2;

/// <summary>
/// Uploads private objects to Cloudflare R2 through the S3-compatible API.
/// </summary>
public class R2ObjectStorageService : IR2ObjectStorageService
{
    private const string SERVICE_NAME = "s3";
    private const string REGION = "auto";
    private const string TERMINATOR = "aws4_request";
    private const string SIGNING_ALGORITHM = "AWS4-HMAC-SHA256";
    private const string AMZ_DATE_FORMAT = "yyyyMMddTHHmmssZ";
    private const string DATE_STAMP_FORMAT = "yyyyMMdd";
    private const string X_AMZ_DATE_HEADER = "x-amz-date";
    private const string X_AMZ_CONTENT_SHA256_HEADER = "x-amz-content-sha256";
    private const string AUTHORIZATION_HEADER_NAME = "Authorization";
    private const string SIGNED_HEADERS = "content-type;host;x-amz-content-sha256;x-amz-date";
    private const string CREDENTIAL_SCOPE_FORMAT = "{0}/{1}/{2}/{3}";
    private const string CANONICAL_REQUEST_FORMAT = "PUT\n{0}\n\ncontent-type:{1}\nhost:{2}\nx-amz-content-sha256:{3}\nx-amz-date:{4}\n\n{5}\n{3}";
    private const string STRING_TO_SIGN_FORMAT = "{0}\n{1}\n{2}\n{3}";
    private const string AUTHORIZATION_FORMAT = "{0} Credential={1}/{2}, SignedHeaders={3}, Signature={4}";
    private const string DEFAULT_CONTENT_TYPE = "application/octet-stream";
    private const string DEFAULT_ENDPOINT_FORMAT = "https://{0}.r2.cloudflarestorage.com";
    private const string R2_OPTIONS_MISSING_MESSAGE = "Cloudflare R2 storage settings are not configured.";
    private const string R2_UPLOAD_FAILED_MESSAGE = "Could not upload file to Cloudflare R2";

    private readonly HttpClient _httpClient;
    private readonly R2StorageOptions _options;
    private readonly ILogger<R2ObjectStorageService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="R2ObjectStorageService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for S3-compatible requests.</param>
    /// <param name="options">The configured Cloudflare R2 options.</param>
    /// <param name="logger">The logger used for upload failures.</param>
    public R2ObjectStorageService(
        HttpClient httpClient,
        IOptions<R2StorageOptions> options,
        ILogger<R2ObjectStorageService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Uploads one private object to the configured Cloudflare R2 bucket.
    /// </summary>
    /// <param name="request">The object upload request.</param>
    /// <param name="cancellationToken">The token used to cancel the upload.</param>
    /// <returns>The private object metadata safe to persist.</returns>
    public async Task<R2ObjectUploadResponse> UploadAsync(
        R2ObjectUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        try
        {
            await using var payload = await CopyToMemoryAsync(request.Content, cancellationToken);
            var checksum = Convert.ToHexString(SHA256.HashData(payload.ToArray())).ToLowerInvariant();
            payload.Position = 0;

            var contentType = string.IsNullOrWhiteSpace(request.ContentType)
                ? DEFAULT_CONTENT_TYPE
                : request.ContentType.Trim();
            var uploadUri = BuildUploadUri(request.ObjectKey);
            using var message = new HttpRequestMessage(HttpMethod.Put, uploadUri)
            {
                Content = new StreamContent(payload)
            };
            message.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            AddAuthorizationHeaders(message, uploadUri, request.ObjectKey, contentType, checksum);

            using var response = await _httpClient.SendAsync(message, cancellationToken);
            response.EnsureSuccessStatusCode();

            var upload = new R2ObjectUploadResponse
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
    /// Validates that all required R2 options are present before attempting a signed upload.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required R2 options are missing.</exception>
    private void EnsureConfigured()
    {
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
        var memory = new MemoryStream();
        await source.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        return memory;
    }

    /// <summary>
    /// Builds the S3-compatible upload URI for the configured bucket and object key.
    /// </summary>
    /// <param name="objectKey">The private object key.</param>
    /// <returns>The absolute R2 upload URI.</returns>
    private Uri BuildUploadUri(string objectKey)
    {
        var endpoint = string.IsNullOrWhiteSpace(_options.Endpoint)
            ? string.Format(DEFAULT_ENDPOINT_FORMAT, _options.AccountId)
            : _options.Endpoint.TrimEnd('/');
        var canonicalPath = BuildCanonicalPath(objectKey);

        return new Uri($"{endpoint}/{_options.BucketName}{canonicalPath}");
    }

    /// <summary>
    /// Adds AWS Signature Version 4 headers required by Cloudflare R2.
    /// </summary>
    /// <param name="message">The outgoing HTTP request.</param>
    /// <param name="uploadUri">The absolute upload URI.</param>
    /// <param name="objectKey">The object key being uploaded.</param>
    /// <param name="contentType">The request content type.</param>
    /// <param name="payloadHash">The SHA-256 payload hash.</param>
    private void AddAuthorizationHeaders(
        HttpRequestMessage message,
        Uri uploadUri,
        string objectKey,
        string contentType,
        string payloadHash)
    {
        var now = DateTime.UtcNow;
        var amzDate = now.ToString(AMZ_DATE_FORMAT, CultureInfo.InvariantCulture);
        var dateStamp = now.ToString(DATE_STAMP_FORMAT, CultureInfo.InvariantCulture);
        var credentialScope = string.Format(
            CREDENTIAL_SCOPE_FORMAT,
            dateStamp,
            REGION,
            SERVICE_NAME,
            TERMINATOR);
        var canonicalRequest = string.Format(
            CANONICAL_REQUEST_FORMAT,
            $"/{_options.BucketName}{BuildCanonicalPath(objectKey)}",
            contentType,
            uploadUri.Host,
            payloadHash,
            amzDate,
            SIGNED_HEADERS);
        var stringToSign = string.Format(
            STRING_TO_SIGN_FORMAT,
            SIGNING_ALGORITHM,
            amzDate,
            credentialScope,
            HashHex(canonicalRequest));
        var signature = CalculateSignature(dateStamp, stringToSign);

        message.Headers.TryAddWithoutValidation(X_AMZ_DATE_HEADER, amzDate);
        message.Headers.TryAddWithoutValidation(X_AMZ_CONTENT_SHA256_HEADER, payloadHash);
        message.Headers.TryAddWithoutValidation(
            AUTHORIZATION_HEADER_NAME,
            string.Format(
                AUTHORIZATION_FORMAT,
                SIGNING_ALGORITHM,
                _options.AccessKeyId,
                credentialScope,
                SIGNED_HEADERS,
                signature));

        // Host is part of the canonical signed headers, so keep it explicit.
        message.Headers.Host = uploadUri.Host;
    }

    /// <summary>
    /// Calculates the AWS Signature Version 4 signature for the supplied string-to-sign.
    /// </summary>
    /// <param name="dateStamp">The YYYYMMDD date stamp.</param>
    /// <param name="stringToSign">The canonical string-to-sign.</param>
    /// <returns>The lowercase hexadecimal signature.</returns>
    private string CalculateSignature(string dateStamp, string stringToSign)
    {
        var dateKey = HmacSha256(Encoding.UTF8.GetBytes($"AWS4{_options.SecretAccessKey}"), dateStamp);
        var regionKey = HmacSha256(dateKey, REGION);
        var serviceKey = HmacSha256(regionKey, SERVICE_NAME);
        var signingKey = HmacSha256(serviceKey, TERMINATOR);

        return Convert.ToHexString(HmacSha256(signingKey, stringToSign)).ToLowerInvariant();
    }

    /// <summary>
    /// Builds the canonical URI path for one object key.
    /// </summary>
    /// <param name="objectKey">The private object key.</param>
    /// <returns>The canonical URI path beginning with a slash.</returns>
    private static string BuildCanonicalPath(string objectKey)
    {
        var segments = objectKey
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString);

        return $"/{string.Join('/', segments)}";
    }

    /// <summary>
    /// Hashes text with SHA-256 and returns lowercase hexadecimal output.
    /// </summary>
    /// <param name="value">The value to hash.</param>
    /// <returns>The lowercase hexadecimal SHA-256 hash.</returns>
    private static string HashHex(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    /// <summary>
    /// Computes one HMAC-SHA256 block.
    /// </summary>
    /// <param name="key">The binary HMAC key.</param>
    /// <param name="data">The text data to sign.</param>
    /// <returns>The HMAC-SHA256 output bytes.</returns>
    private static byte[] HmacSha256(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);

        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }
}
