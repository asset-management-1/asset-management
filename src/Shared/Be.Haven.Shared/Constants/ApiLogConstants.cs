namespace Be.Haven.Shared.Constants;

public static class ApiLogConstants
{
    /// <summary>
    /// Log templates related to API versioning and related errors.
    /// </summary>
    public static class ApiVersionLogs
    {
        /// <summary>
        /// Logged when an HTTP error response is generated. Includes details about the
        /// HTTP status code, specific error code, and the request path to aid in debugging
        /// and error tracing.
        /// </summary>
        public const string LOG_HTTP_ERROR_GENERATED = "HTTP error response generated. StatusCode={StatusCode}, ErrorCode={ErrorCode}, Path={Path}";

        /// <summary>
        /// Logged when an API version mismatch is detected. Provides details about the requested version
        /// and the corresponding request path to help identify discrepancies.
        /// </summary>
        public const string LOG_VERSION_MISMATCH = "API version mismatch detected. RequestedVersion=v{RequestedVersion}, Path={Path}";
    }
    
    /// <summary>
    /// Log templates for NA token authentication lifecycle.
    /// </summary>
    public static class NaAuthenticationLogs
    {
        /// <summary>
        /// Logged when authentication is skipped for health check endpoint.
        /// </summary>
        public const string LOG_AUTH_SKIPPED_HEALTH = "NA auth skipped for health check endpoint. Path={Path}, Method={Method}";
        
        /// <summary>
        /// Logged when authentication is failed due to missing or invalid Authorization header.
        /// </summary>
        public const string LOG_AUTH_FAILED = "NA token validation failed: missing/invalid Authorization header. Path={Path}, Method={Method}";

        /// <summary>
        /// Logged when token validation process starts.
        /// </summary>
        public const string LOG_AUTH_STARTED = "NA token validation started. Path={Path}, Method={Method}";

        /// <summary>
        /// Logged when token validation succeeds and user identity is resolved.
        /// </summary>
        public const string LOG_AUTH_SUCCEEDED = "NA token validation succeeded. UserName={UserName}";

        /// <summary>
        /// Logged when token validation fails due to expired token.
        /// </summary>
        public const string LOG_AUTH_REJECTED_EXPIRED = "NA token validation rejected: expired token.";

        /// <summary>
        /// Logged when token validation fails for any other reason.
        /// </summary>
        public const string LOG_AUTH_REJECTED = "NA token validation rejected. ErrorType={ErrorType}, Error={Error}";

        /// <summary>
        /// Logged when a 401 Unauthorized challenge response is returned.
        /// </summary>
        public const string LOG_CHALLENGE_401 = "NA auth response: challenge issued (401).";

        /// <summary>
        /// Logged when a 403 Forbidden response is returned.
        /// </summary>
        public const string LOG_FORBIDDEN_403 = "NA auth response: access forbidden (403).";

        /// <summary>
        /// Logged when error payload is written to response body.
        /// </summary>
        public const string LOG_WRITE_ERROR_PAYLOAD = "NA auth response: writing error payload. StatusCode={StatusCode}, Code={Code}, Message={Message}";

        /// <summary>
        /// Logged when OIDC configuration is successfully loaded from well-known endpoint.
        /// </summary>
        public const string LOG_OIDC_LOADED = "NA token validation: OIDC config loaded. Issuer={Issuer}";

        /// <summary>
        /// Logged when JWT token is successfully validated.
        /// </summary>
        public const string LOG_JWT_VALIDATED = "NA token validation: JWT validated successfully.";

        /// <summary>
        /// Logged when claims are normalized after successful validation.
        /// </summary>
        public const string LOG_CLAIMS_NORMALIZED = "NA token validation: claims normalized.";
    }

    /// <summary>
    /// Log templates for Haven token authentication lifecycle.
    /// </summary>
    public static class HavenAuthenticationLogs
    {
        /// <summary>
        /// Logged when authentication is skipped for the health endpoint.
        /// </summary>
        public const string LOG_AUTH_SKIPPED_HEALTH = "Haven auth skipped for health endpoint. Path={Path}, Method={Method}";

        /// <summary>
        /// Logged when a protected endpoint is called without a valid bearer header.
        /// </summary>
        public const string LOG_AUTH_FAILED = "Haven token validation failed: missing/invalid Authorization header. Path={Path}, Method={Method}";

        /// <summary>
        /// Logged when Haven token validation begins.
        /// </summary>
        public const string LOG_AUTH_STARTED = "Haven token validation started. Path={Path}, Method={Method}";

        /// <summary>
        /// Logged when Haven token validation succeeds.
        /// </summary>
        public const string LOG_AUTH_SUCCEEDED = "Haven token validation succeeded. UserId={UserId}";

        /// <summary>
        /// Logged when Haven token validation fails because the token is expired.
        /// </summary>
        public const string LOG_AUTH_REJECTED_EXPIRED = "Haven token validation rejected: expired token.";

        /// <summary>
        /// Logged when Haven token validation fails for any other reason.
        /// </summary>
        public const string LOG_AUTH_REJECTED = "Haven token validation rejected. ErrorType={ErrorType}, Error={Error}";

        /// <summary>
        /// Logged when a 401 challenge response is written by the Haven authentication handler.
        /// </summary>
        public const string LOG_CHALLENGE_401 = "Haven auth response: challenge issued (401).";

        /// <summary>
        /// Logged when a 403 forbidden response is written by the Haven authentication handler.
        /// </summary>
        public const string LOG_FORBIDDEN_403 = "Haven auth response: access forbidden (403).";

        /// <summary>
        /// Logged when the Haven authentication handler writes an error payload.
        /// </summary>
        public const string LOG_WRITE_ERROR_PAYLOAD = "Haven auth response: writing error payload. StatusCode={StatusCode}, Code={Code}, Message={Message}";
    }
}
