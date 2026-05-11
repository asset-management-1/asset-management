namespace Be.Haven.Shared.Constants;

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
    /// Identifier for the type or category of a National Address (NA) user, used for role-based or access-based differentiation.
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
    /// Identifier representing an individual user in the context of National Address (NA) operations or role-based access systems.
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
    /// Centralized user-facing/system log messages for common auth failures.
    /// </summary>
    public static class SystemMessage
    {
        /// <summary>
        /// Message when the Haven bearer token is expired.
        /// </summary>
        public const string TOKEN_EXPIRED = "Token expired.";

        /// <summary>
        /// Message when the Haven bearer token fails validation.
        /// </summary>
        public const string INVALID_TOKEN = "Invalid token.";

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
