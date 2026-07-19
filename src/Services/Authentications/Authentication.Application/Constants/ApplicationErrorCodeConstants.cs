namespace Authentication.Application.Constants;

/// <summary>
/// Stores authentication application error codes grouped by owning workflow.
/// </summary>
public static partial class ApplicationErrorConstants
{
    /// <summary>
    /// Contains account and identity error codes.
    /// </summary>
    public static class AccountErrorCodes
    {
        /// <summary>
        /// Error code returned when supplied login credentials are invalid.
        /// </summary>
        public const string AUTH_INVALID_CREDENTIALS = "error_auth_invalid_credentials";

        /// <summary>
        /// Error code returned when a user account already exists for the requested identity data.
        /// </summary>
        public const string AUTH_USER_ALREADY_EXISTS = "error_auth_user_already_exists";

        /// <summary>
        /// Error code returned when the required user-status master data cannot be found.
        /// </summary>
        public const string AUTH_USER_STATUS_NOT_FOUND = "error_auth_user_status_not_found";

        /// <summary>
        /// Error code returned when the requested authentication operation is forbidden.
        /// </summary>
        public const string AUTH_FORBIDDEN_OPERATION = "error_auth_forbidden_operation";

        /// <summary>
        /// Error code returned when an account has no active Party context for token issuance.
        /// </summary>
        public const string AUTH_PARTY_CONTEXT_NOT_AVAILABLE = "error_auth_party_context_not_available";

        /// <summary>
        /// Error code returned when the target account is inactive.
        /// </summary>
        public const string AUTH_ACCOUNT_INACTIVE = "error_auth_account_inactive";
    }

    /// <summary>
    /// Contains token and device error codes.
    /// </summary>
    public static class TokenErrorCodes
    {
        /// <summary>
        /// Error code returned when a refresh token is invalid, expired, or revoked.
        /// </summary>
        public const string AUTH_INVALID_REFRESH_TOKEN = "error_auth_invalid_refresh_token";

        /// <summary>
        /// Error code returned when a refresh request reuses the immediate previous token hash.
        /// </summary>
        public const string AUTH_REFRESH_DUPLICATE = "error_auth_refresh_duplicate";

        /// <summary>
        /// Error code returned when token issuance cannot capture required client device metadata.
        /// </summary>
        public const string AUTH_DEVICE_INFO_REQUIRED = "error_auth_device_info_required";
    }

    /// <summary>
    /// Contains OTP, registration, and reset-session error codes.
    /// </summary>
    public static class OtpErrorCodes
    {
        /// <summary>
        /// Error code returned when an OTP is invalid or expired.
        /// </summary>
        public const string AUTH_OTP_INVALID = "error_auth_otp_invalid";

        /// <summary>
        /// Error code returned when an OTP request is still in cooldown.
        /// </summary>
        public const string AUTH_OTP_COOLDOWN = "error_auth_otp_cooldown";

        /// <summary>
        /// Error code returned when OTP request rate limits are exceeded.
        /// </summary>
        public const string AUTH_OTP_RATE_LIMIT = "error_auth_otp_rate_limit";

        /// <summary>
        /// Error code returned when pending registration data is missing or expired.
        /// </summary>
        public const string AUTH_PENDING_REGISTER_NOT_FOUND = "error_auth_pending_register_not_found";

        /// <summary>
        /// Error code returned when the forgot-password reset session is invalid or expired.
        /// </summary>
        public const string AUTH_RESET_SESSION_INVALID = "error_auth_reset_session_invalid";
    }

    /// <summary>
    /// Contains external identity provider error codes.
    /// </summary>
    public static class ExternalProviderErrorCodes
    {
        /// <summary>
        /// Error code returned when an external identity provider is invalid or unsupported.
        /// </summary>
        public const string AUTH_EXTERNAL_PROVIDER_INVALID = "error_auth_external_provider_invalid";

        /// <summary>
        /// Error code returned when an external provider is already linked to another account.
        /// </summary>
        public const string AUTH_EXTERNAL_PROVIDER_LINK_CONFLICT = "error_auth_external_provider_link_conflict";

        /// <summary>
        /// Error code returned when the requested external provider is not linked to the current account.
        /// </summary>
        public const string AUTH_EXTERNAL_PROVIDER_NOT_LINKED = "error_auth_external_provider_not_linked";

        /// <summary>
        /// Error code returned when unlinking would remove the final usable sign-in method.
        /// </summary>
        public const string AUTH_LAST_SIGN_IN_METHOD_REQUIRED = "error_auth_last_sign_in_method_required";

        /// <summary>
        /// Error code returned when provider validation is temporarily unavailable.
        /// </summary>
        public const string AUTH_EXTERNAL_PROVIDER_UNAVAILABLE = "error_auth_external_provider_unavailable";

        /// <summary>
        /// Error code returned when the provider does not supply the required email.
        /// </summary>
        public const string AUTH_EXTERNAL_EMAIL_REQUIRED = "error_auth_external_email_required";
    }

    /// <summary>
    /// Contains profile, avatar, and gender error codes.
    /// </summary>
    public static class ProfileErrorCodes
    {
        /// <summary>
        /// Error code returned when profile input contains an unsupported gender value.
        /// </summary>
        public const string AUTH_INVALID_GENDER = "error_auth_invalid_gender";

        /// <summary>
        /// Error code returned when profile avatar upload fails.
        /// </summary>
        public const string AUTH_PROFILE_UPLOAD_FAILED = "error_auth_profile_upload_failed";

        /// <summary>
        /// Error code returned when profile update persistence fails after upload work succeeds.
        /// </summary>
        public const string AUTH_PROFILE_UPDATE_FAILED = "error_auth_profile_update_failed";
    }

    /// <summary>
    /// Contains KYC error codes.
    /// </summary>
    public static class KycErrorCodes
    {
        /// <summary>
        /// Error code returned when KYC submission data is invalid.
        /// </summary>
        public const string AUTH_KYC_INVALID = "error_auth_kyc_invalid";

        /// <summary>
        /// Error code returned when private KYC document upload fails.
        /// </summary>
        public const string AUTH_KYC_UPLOAD_FAILED = "error_auth_kyc_upload_failed";
    }
}
