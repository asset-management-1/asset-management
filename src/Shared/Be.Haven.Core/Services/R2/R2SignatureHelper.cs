namespace Be.Haven.Core.Services.R2;

/// <summary>
/// Builds Cloudflare R2 S3-compatible Signature Version 4 request values.
/// </summary>
internal static class R2SignatureHelper
{
    /// <summary>
    /// Builds AWS Signature Version 4 headers required for a Cloudflare R2 upload request.
    /// </summary>
    /// <param name="options">The configured Cloudflare R2 options.</param>
    /// <param name="uploadUri">The absolute upload URI.</param>
    /// <param name="objectKey">The private object key being uploaded.</param>
    /// <param name="contentType">The request content type.</param>
    /// <param name="payloadHash">The SHA-256 payload hash.</param>
    /// <returns>The signed R2 upload request headers.</returns>
    internal static Dictionary<string, string> BuildUploadHeaders(
        R2StorageOptions options,
        Uri uploadUri,
        string objectKey,
        string contentType,
        string payloadHash)
    {
        var signingContext = BuildSigningContext();
        var canonicalRequest = string.Format(
            R2_UPLOAD_CANONICAL_REQUEST_FORMAT,
            $"/{options.BucketName}{BuildCanonicalPath(objectKey)}",
            contentType,
            uploadUri.Host,
            payloadHash,
            signingContext.AmzDate,
            R2_UPLOAD_SIGNED_HEADERS);

        return BuildAuthorizationHeaders(
            options,
            signingContext,
            R2_UPLOAD_SIGNED_HEADERS,
            HashHex(canonicalRequest),
            payloadHash);
    }

    /// <summary>
    /// Builds AWS Signature Version 4 headers required for a Cloudflare R2 delete request.
    /// </summary>
    /// <param name="options">The configured Cloudflare R2 options.</param>
    /// <param name="deleteUri">The absolute delete URI.</param>
    /// <param name="objectKey">The private object key being deleted.</param>
    /// <returns>The signed R2 delete request headers.</returns>
    internal static Dictionary<string, string> BuildDeleteHeaders(
        R2StorageOptions options,
        Uri deleteUri,
        string objectKey)
    {
        var signingContext = BuildSigningContext();
        var canonicalRequest = string.Format(
            R2_DELETE_CANONICAL_REQUEST_FORMAT,
            $"/{options.BucketName}{BuildCanonicalPath(objectKey)}",
            deleteUri.Host,
            R2_EMPTY_PAYLOAD_HASH,
            signingContext.AmzDate,
            R2_DELETE_SIGNED_HEADERS);

        return BuildAuthorizationHeaders(
            options,
            signingContext,
            R2_DELETE_SIGNED_HEADERS,
            HashHex(canonicalRequest),
            R2_EMPTY_PAYLOAD_HASH);
    }

    /// <summary>
    /// Builds the canonical URI path for one object key.
    /// </summary>
    /// <param name="objectKey">The private object key.</param>
    /// <returns>The canonical URI path beginning with a slash.</returns>
    internal static string BuildCanonicalPath(string objectKey)
    {
        var segments = objectKey
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString);

        return $"/{string.Join('/', segments)}";
    }

    /// <summary>
    /// Builds common SigV4 date and credential-scope values.
    /// </summary>
    /// <returns>The signing context used by upload and delete requests.</returns>
    private static (string AmzDate, string DateStamp, string CredentialScope) BuildSigningContext()
    {
        var now = DateTime.UtcNow;
        var dateStamp = now.ToString(R2_DATE_STAMP_FORMAT, CultureInfo.InvariantCulture);

        return (
            now.ToString(R2_AMZ_DATE_FORMAT, CultureInfo.InvariantCulture),
            dateStamp,
            string.Format(
                R2_CREDENTIAL_SCOPE_FORMAT,
                dateStamp,
                R2_SIGNATURE_REGION,
                R2_SIGNATURE_SERVICE_NAME,
                R2_SIGNATURE_TERMINATOR));
    }

    /// <summary>
    /// Builds the final authorization and payload-hash headers for one signed R2 request.
    /// </summary>
    /// <param name="options">The configured Cloudflare R2 options.</param>
    /// <param name="signingContext">The shared SigV4 signing context.</param>
    /// <param name="signedHeaders">The signed header list for the request.</param>
    /// <param name="canonicalRequestHash">The lowercase hash of the canonical request.</param>
    /// <param name="payloadHash">The lowercase payload hash sent to R2.</param>
    /// <returns>The signed R2 request headers.</returns>
    private static Dictionary<string, string> BuildAuthorizationHeaders(
        R2StorageOptions options,
        (string AmzDate, string DateStamp, string CredentialScope) signingContext,
        string signedHeaders,
        string canonicalRequestHash,
        string payloadHash)
    {
        var stringToSign = string.Format(
            R2_STRING_TO_SIGN_FORMAT,
            R2_SIGNATURE_ALGORITHM,
            signingContext.AmzDate,
            signingContext.CredentialScope,
            canonicalRequestHash);
        var signature = CalculateSignature(options, signingContext.DateStamp, stringToSign);

        return new Dictionary<string, string>
        {
            { R2_AMZ_DATE_HEADER, signingContext.AmzDate },
            { R2_AMZ_CONTENT_SHA256_HEADER, payloadHash },
            {
                AUTHORIZATION_HEADER,
                string.Format(
                    R2_AUTHORIZATION_FORMAT,
                    R2_SIGNATURE_ALGORITHM,
                    options.AccessKeyId,
                    signingContext.CredentialScope,
                    signedHeaders,
                    signature)
            }
        };
    }

    /// <summary>
    /// Calculates the AWS Signature Version 4 signature for the supplied string-to-sign.
    /// </summary>
    /// <param name="options">The configured Cloudflare R2 options.</param>
    /// <param name="dateStamp">The YYYYMMDD date stamp.</param>
    /// <param name="stringToSign">The canonical string-to-sign.</param>
    /// <returns>The lowercase hexadecimal signature.</returns>
    private static string CalculateSignature(R2StorageOptions options, string dateStamp, string stringToSign)
    {
        var dateKey = HmacSha256(UTF8.GetBytes($"AWS4{options.SecretAccessKey}"), dateStamp);
        var regionKey = HmacSha256(dateKey, R2_SIGNATURE_REGION);
        var serviceKey = HmacSha256(regionKey, R2_SIGNATURE_SERVICE_NAME);
        var signingKey = HmacSha256(serviceKey, R2_SIGNATURE_TERMINATOR);

        return Convert.ToHexString(HmacSha256(signingKey, stringToSign)).ToLowerInvariant();
    }

    /// <summary>
    /// Hashes text with SHA-256 and returns lowercase hexadecimal output.
    /// </summary>
    /// <param name="value">The value to hash.</param>
    /// <returns>The lowercase hexadecimal SHA-256 hash.</returns>
    private static string HashHex(string value) =>
        Convert.ToHexString(SHA256.HashData(UTF8.GetBytes(value))).ToLowerInvariant();

    /// <summary>
    /// Computes one HMAC-SHA256 block.
    /// </summary>
    /// <param name="key">The binary HMAC key.</param>
    /// <param name="data">The text data to sign.</param>
    /// <returns>The HMAC-SHA256 output bytes.</returns>
    private static byte[] HmacSha256(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);

        return hmac.ComputeHash(UTF8.GetBytes(data));
    }
}
