namespace Be.Haven.Shared.Constants.Authentication;

public static class AuthConstants
{
    /// <summary>
    /// Configuration section key that contains Haven authentication/JWT settings.
    /// </summary>
    public const string AUTH_SETTINGS = "AuthSettings";

    /// <summary>
    /// Header or context key used to carry the upstream NA bearer token.
    /// </summary>
    public const string NA_TOKEN = "NA_TOKEN";

    /// <summary>
    /// NA-specific Unix expiry (seconds) copied from SPL token to enforce equal validity windows.
    /// </summary>
    public const string NA_EXP = "na_exp";

    /// <summary>
    /// NA-specific Unix not-before (seconds) copied from SPL token to defer validity start.
    /// </summary>
    public const string NA_NBF = "na_nbf";

    /// <summary>
    /// Identifier for the type or category of a Haven user, used for role-based or access-based differentiation.
    /// </summary>
    public const string NA_USER_TYPE = "na_user_type";

    /// <summary>
    /// SOAP/WS-Identity claim for unique user identifier (nameidentifier).
    /// </summary>
    public const string SOAP_NAME_ID = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

    /// <summary>
    /// SOAP/WS-Identity claim for username (display name).
    /// </summary>
    public const string SOAP_NAME = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

    /// <summary>
    /// SOAP/WS-Identity claim for email address.
    /// </summary>
    public const string SOAP_EMAIL = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";

    /// <summary>
    /// NA-standard claim for user id (alias of SOAP_NAME_ID).
    /// </summary>
    public const string NA_USER_ID = "na_user_id";

    /// <summary>
    /// NA-standard claim for username (alias of SOAP_NAME).
    /// </summary>
    public const string NA_USERNAME = "na_username";

    /// <summary>
    /// NA-standard claim for role/roles (application-specific).
    /// </summary>
    public const string NA_ROLE = "na_role";

    /// <summary>
    /// NA-standard claim for email (alias of SOAP_EMAIL).
    /// </summary>
    public const string NA_EMAIL = "na_email";

    /// <summary>
    /// Claim key representing the mobile phone number in English, associated with the user's account.
    /// Example source: Claims within the authenticated user's token.
    /// </summary>
    public const string NA_MOBILE_PHONE = "na_mobile_phone";

    /// <summary>
    /// Claim key representing the national identifier associated with the user in the caller's token.
    /// Example source: NA-issued access token claims.
    /// </summary>
    public const string NA_NATIONAL_ID = "na_national_id";

    /// <summary>
    /// Attribute key for the internal factory identifier.
    /// </summary>
    public const string NA_FACTORY_ID = "na_factory_id";

    /// <summary>
    /// Attribute key used on the NA/Keycloak side to store the mapped customer id.
    /// Note the lower-camel case to match NA attribute naming conventions.
    /// </summary>
    public const string NA_CUSTOMER_ID = "customer_id";

    /// <summary>
    /// Claim key for the user's first name in English.
    /// </summary>
    public const string NA_FIRST_NAME_EN = "na_first_name_en";

    /// <summary>
    /// Header name for account id in Authentication Service.
    /// </summary>
    public const string NA_ACCOUNT_ID = "account_id";

    /// <summary>
    /// Claim key for the user's third name in English.
    /// </summary>
    public const string NA_THIRD_NAME_EN = "na_third_name_en";

    /// <summary>
    /// Claim key for the user's last name in English.
    /// </summary>
    public const string NA_LAST_NAME_EN = "na_last_name_en";

    /// <summary>
    /// Claim key for the user's full name in English.
    /// </summary>
    public const string NA_FULLNAME_EN = "full_name_en";

    /// <summary>
    /// Claim key for CRM email associated with the user.
    /// </summary>
    public const string NA_CRM_EMAIL = "na_crm_email";

    /// <summary>
    /// Claim key for the user's full name in Arabic.
    /// </summary>
    public const string NA_FULLNAME_AR = "full_name_ar";

    /// <summary>
    /// Claim key for CRM mobile phone associated with the user.
    /// </summary>
    public const string NA_CRM_MOBILE_PHONE = "na_crm_mobile_phone";

    /// <summary>
    /// Claim key for the user's first name in Arabic.
    /// </summary>
    public const string NA_FIRST_NAME_AR = "fist_name_ar";

    /// <summary>
    /// Claim key for the user's third name in Arabic.
    /// </summary>
    public const string NA_THIRD_NAME_AR = "third_name_ar";

    /// <summary>
    /// Claim key for the user's last name in Arabic.
    /// </summary>
    public const string NA_LAST_NAME_AR = "last_name_ar";

    /// <summary>
    /// NA-standard claim for JWT identifier.
    /// </summary>
    public const string NA_JTI = "jti";

    /// <summary>
    /// Identifier representing an individual user in the context of Haven (NA) operations or role-based access systems.
    /// </summary>
    public const string INDIVIDUAL = "Individual";

    /// <summary>
    /// Header name for account id in Registration Service.
    /// </summary>
    public const string HEADER_ACCOUNT_ID = "X-Account-Id";

    /// <summary>
    /// Standard and application-specific claim keys used across authentication flows.
    /// </summary>
    public static class TokenClaimTypes
    {
        /// <summary>
        /// Haven-issued access-token timestamp in Unix milliseconds.
        /// </summary>
        public const string HAVEN_ISSUED_AT_MS = "haven_issued_at_ms";

        /// <summary>
        /// Server-issued public identifier for the authenticated client session.
        /// </summary>
        public const string SESSION_ID = "session_id";

        /// <summary>
        /// OpenID Connect subject claim.
        /// </summary>
        public const string SUBJECT = "sub";

        /// <summary>
        /// OpenID Connect name claim.
        /// </summary>
        public const string NAME = "name";

        /// <summary>
        /// OpenID Connect email claim.
        /// </summary>
        public const string EMAIL = "email";

        /// <summary>
        /// OpenID Connect given-name claim.
        /// </summary>
        public const string GIVEN_NAME = "given_name";

        /// <summary>
        /// OpenID Connect email-verified claim.
        /// </summary>
        public const string EMAIL_VERIFIED = "email_verified";

        /// <summary>
        /// Application claim used to carry account identifier.
        /// </summary>
        public const string ACCOUNT_ID = "account_id";

        /// <summary>
        /// Standard single-role claim key.
        /// </summary>
        public const string ROLE = "role";

        /// <summary>
        /// Standard multi-role claim key.
        /// </summary>
        public const string ROLES = "roles";

        /// <summary>
        /// OAuth/OIDC scope claim key.
        /// </summary>
        public const string SCOPE = "scope";

        /// <summary>
        /// Application permission claim key.
        /// </summary>
        public const string PERMISSION = "permission";
    }

    /// <summary>
    /// Header names used by clients to submit non-authoritative device metadata.
    /// </summary>
    public static class ClientDeviceHeaders
    {
        /// <summary>
        /// Required client-device metadata headers for endpoints that issue authentication tokens.
        /// </summary>
        public static readonly IReadOnlyList<string> REQUIRED_HEADERS =
        [
            DEVICE_ID,
            DEVICE_NAME,
            DEVICE_TYPE,
            USER_AGENT
        ];

        /// <summary>
        /// Header carrying a stable client-generated app/browser instance identifier.
        /// </summary>
        public const string DEVICE_ID = "X-Device-Id";

        /// <summary>
        /// Header carrying a client display name for the current device.
        /// </summary>
        public const string DEVICE_NAME = "X-Device-Name";

        /// <summary>
        /// Header carrying the client device category, such as mobile or web.
        /// </summary>
        public const string DEVICE_TYPE = "X-Device-Type";

        /// <summary>
        /// Standard HTTP header carrying the user-agent string.
        /// </summary>
        public const string USER_AGENT = "User-Agent";
    }

    /// <summary>
    /// Maximum lengths for stored and tokenized client device metadata.
    /// </summary>
    public static class ClientDeviceMetadataLimits
    {
        /// <summary>
        /// Maximum stored length for a client instance identifier.
        /// </summary>
        public const int DEVICE_ID_MAX_LENGTH = 150;

        /// <summary>
        /// Maximum stored length for a client device display name.
        /// </summary>
        public const int DEVICE_NAME_MAX_LENGTH = 150;

        /// <summary>
        /// Maximum stored length for a client device type.
        /// </summary>
        public const int DEVICE_TYPE_MAX_LENGTH = 50;

        /// <summary>
        /// Maximum stored length for a user-agent value.
        /// </summary>
        public const int USER_AGENT_MAX_LENGTH = 512;

        /// <summary>
        /// Maximum stored length for a remote IP address value.
        /// </summary>
        public const int IP_ADDRESS_MAX_LENGTH = 64;
    }

    /// <summary>
    /// Fallback values used when client device metadata is omitted.
    /// </summary>
    public static class ClientDeviceFallbacks
    {
        /// <summary>
        /// Fallback device identifier when optional metadata reads do not include a client instance id.
        /// </summary>
        public const string DEVICE_ID = "unknown-device";

        /// <summary>
        /// Fallback device display name when the client does not send one.
        /// </summary>
        public const string DEVICE_NAME = "Unknown device";

        /// <summary>
        /// Fallback device type when the client does not send one.
        /// </summary>
        public const string DEVICE_TYPE = "unknown";

        /// <summary>
        /// Fallback user-agent value when the request does not contain one.
        /// </summary>
        public const string USER_AGENT = "unknown";

        /// <summary>
        /// Fallback remote IP value when the request context cannot provide one.
        /// </summary>
        public const string IP_ADDRESS = "unknown";
    }

    /// <summary>
    /// Redis key pattern for the per-user authentication reset timestamp.
    /// </summary>
    public const string AUTH_RESET_AT_KEY_PATTERN = "auth:auth-reset-at:{0}";

    /// <summary>
    /// Extra cache lifetime, in days, added to refresh-token lifetime for auth reset markers.
    /// </summary>
    public const int AUTH_RESET_CACHE_TTL_PADDING_DAYS = 1;

    /// <summary>
    /// Default access-token lifetime in minutes.
    /// </summary>
    public const int DEFAULT_ACCESS_TOKEN_MINUTES = 60;

    /// <summary>
    /// Default refresh-token lifetime in days.
    /// </summary>
    public const int DEFAULT_REFRESH_TOKEN_DAYS = 7;

    /// <summary>
    /// Centralized user-facing/system log messages for common auth failures.
    /// </summary>
    public static class SystemMessage
    {
        /// <summary>
        /// Message used when shared Haven token validation settings are incomplete or unsafe for the current environment.
        /// </summary>
        public const string AUTH_OPTIONS_INVALID = "AuthSettings must include issuer, audience, and a valid secret for the current environment.";

        /// <summary>
        /// Message when the Haven bearer token is expired.
        /// </summary>
        public const string TOKEN_EXPIRED = "Token expired.";

        /// <summary>
        /// Message when the Haven bearer token fails validation.
        /// </summary>
        public const string INVALID_TOKEN = "Invalid token.";

        /// <summary>
        /// Message when authentication reset state cannot be validated.
        /// </summary>
        public const string AUTH_STATE_UNAVAILABLE = "Authentication state is temporarily unavailable.";

        /// <summary>
        /// Message when the NA bearer token is missing or malformed.
        /// </summary>
        public const string MISSING_NA_ACCESS_TOKEN = "Missing or invalid NA bearer token.";

        /// <summary>
        /// Message when the caller lacks sufficient authorization to access a resource.
        /// </summary>
        public const string MISSING_PERMISSION = "You do not have permission to access this resource.";

        /// <summary>
        /// Message when the NA token fails validation; includes formatted detail (e.g., reason).
        /// Use with <c>string.Format</c> to inject the specific error: <c>string.Format(INVALID_NA_ACCESS_TOKEN, reason)</c>.
        /// </summary>
        public const string INVALID_NA_ACCESS_TOKEN = "Invalid NA token. {0}";

        /// <summary>
        /// Message when the NA token is expired.
        /// </summary>
        public const string EXPIRED_NA_ACCESS_TOKEN = "NA token expired.";
    }
}
