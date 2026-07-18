namespace Be.Haven.Shared.Constants.Storage;

/// <summary>
/// Stores Cloudflare R2 S3-compatible protocol constants.
/// </summary>
public static class R2StorageConstants
{
    /// <summary>
    /// Service name used by Cloudflare R2 Signature Version 4.
    /// </summary>
    public const string R2_SIGNATURE_SERVICE_NAME = "s3";

    /// <summary>
    /// Region value required by Cloudflare R2 Signature Version 4.
    /// </summary>
    public const string R2_SIGNATURE_REGION = "auto";

    /// <summary>
    /// Credential-scope terminator used by Signature Version 4.
    /// </summary>
    public const string R2_SIGNATURE_TERMINATOR = "aws4_request";

    /// <summary>
    /// Signature Version 4 algorithm name.
    /// </summary>
    public const string R2_SIGNATURE_ALGORITHM = "AWS4-HMAC-SHA256";

    /// <summary>
    /// x-amz-date timestamp format used by Signature Version 4.
    /// </summary>
    public const string R2_AMZ_DATE_FORMAT = "yyyyMMddTHHmmssZ";

    /// <summary>
    /// Date-stamp format used by Signature Version 4 credential scopes.
    /// </summary>
    public const string R2_DATE_STAMP_FORMAT = "yyyyMMdd";

    /// <summary>
    /// Header name for the Signature Version 4 request timestamp.
    /// </summary>
    public const string R2_AMZ_DATE_HEADER = "x-amz-date";

    /// <summary>
    /// Header name for the Signature Version 4 payload hash.
    /// </summary>
    public const string R2_AMZ_CONTENT_SHA256_HEADER = "x-amz-content-sha256";

    /// <summary>
    /// Signed header list used by R2 raw PUT uploads.
    /// </summary>
    public const string R2_UPLOAD_SIGNED_HEADERS = "content-type;host;x-amz-content-sha256;x-amz-date";

    /// <summary>
    /// Signed header list used by R2 object delete calls.
    /// </summary>
    public const string R2_DELETE_SIGNED_HEADERS = "host;x-amz-content-sha256;x-amz-date";

    /// <summary>
    /// Format used to build a Signature Version 4 credential scope.
    /// </summary>
    public const string R2_CREDENTIAL_SCOPE_FORMAT = "{0}/{1}/{2}/{3}";

    /// <summary>
    /// Canonical request format used by R2 raw PUT uploads.
    /// </summary>
    public const string R2_UPLOAD_CANONICAL_REQUEST_FORMAT = "PUT\n{0}\n\ncontent-type:{1}\nhost:{2}\nx-amz-content-sha256:{3}\nx-amz-date:{4}\n\n{5}\n{3}";

    /// <summary>
    /// Canonical request format used by R2 object delete calls.
    /// </summary>
    public const string R2_DELETE_CANONICAL_REQUEST_FORMAT = "DELETE\n{0}\n\nhost:{1}\nx-amz-content-sha256:{2}\nx-amz-date:{3}\n\n{4}\n{2}";

    /// <summary>
    /// String-to-sign format used by Signature Version 4.
    /// </summary>
    public const string R2_STRING_TO_SIGN_FORMAT = "{0}\n{1}\n{2}\n{3}";

    /// <summary>
    /// Authorization header format used by Signature Version 4.
    /// </summary>
    public const string R2_AUTHORIZATION_FORMAT = "{0} Credential={1}/{2}, SignedHeaders={3}, Signature={4}";

    /// <summary>
    /// Default content type used when an uploaded object does not provide one.
    /// </summary>
    public const string R2_DEFAULT_CONTENT_TYPE = "application/octet-stream";

    /// <summary>
    /// SHA-256 hash for an empty payload used by delete requests.
    /// </summary>
    public const string R2_EMPTY_PAYLOAD_HASH = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

}
