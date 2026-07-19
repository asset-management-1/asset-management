namespace Authentication.Application.Constants;

/// <summary>
/// Stores authentication application error messages grouped by owning workflow.
/// </summary>
public static partial class ApplicationErrorConstants
{
    /// <summary>
    /// Contains shared validation error messages.
    /// </summary>
    public static class ValidationErrors
    {
        /// <summary>
        /// Validation message returned when a password does not satisfy complexity rules.
        /// </summary>
        public const string PASSWORD_COMPLEXITY_RULES = "Password must meet complexity rules (min 8 chars, include uppercase, lowercase, number, and special character).";

        /// <summary>
        /// Validation message returned when the new password equals the current password.
        /// </summary>
        public const string NEW_PASSWORD_MUST_DIFFER_FROM_CURRENT_PASSWORD = "NewPassword must be different from CurrentPassword.";
    }

    /// <summary>
    /// Contains party-context and master-data error messages.
    /// </summary>
    public static class ContextErrors
    {
        /// <summary>
        /// Generic message returned when the current authenticated user context cannot be resolved.
        /// </summary>
        public const string UNAUTHORIZED_REQUEST_MESSAGE = "Unauthorized request.";

        /// <summary>
        /// Message returned when the requested party type is invalid.
        /// </summary>
        public const string INVALID_PARTY_TYPE_MESSAGE = "Invalid party type.";

        /// <summary>
        /// Message returned when required master data cannot be found.
        /// </summary>
        public const string REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE = "Required master data was not found.";

        /// <summary>
        /// Message returned when the active status master-data value cannot be found.
        /// </summary>
        public const string ACTIVE_STATUS_NOT_FOUND_MESSAGE = "Active status was not found.";

        /// <summary>
        /// Message returned when the default external-user party type cannot be found.
        /// </summary>
        public const string DEFAULT_PARTY_TYPE_NOT_FOUND_MESSAGE = "Default party type was not found.";

        /// <summary>
        /// Message returned when the account has no active Party context.
        /// </summary>
        public const string PARTY_CONTEXT_NOT_AVAILABLE_MESSAGE = "No active party context is available for this account.";
    }

    /// <summary>
    /// Contains OTP and session error messages.
    /// </summary>
    public static class OtpErrors
    {
        /// <summary>
        /// Message returned when OTP delivery fails.
        /// </summary>
        public const string OTP_SEND_FAILED_MESSAGE = "Unable to send OTP at this time.";

        /// <summary>
        /// Message returned when pending registration data has expired.
        /// </summary>
        public const string REGISTRATION_SESSION_EXPIRED_MESSAGE = "Registration session has expired.";

        /// <summary>
        /// Message returned when an OTP is invalid or expired.
        /// </summary>
        public const string OTP_INVALID_OR_EXPIRED_MESSAGE = "OTP is invalid or expired.";

        /// <summary>
        /// Message returned when an OTP request is still in cooldown.
        /// </summary>
        public const string OTP_COOLDOWN_MESSAGE = "Please wait before requesting another OTP.";

        /// <summary>
        /// Message returned when OTP request rate limits are exceeded.
        /// </summary>
        public const string OTP_RATE_LIMIT_MESSAGE = "Too many OTP requests. Please try again later.";

        /// <summary>
        /// Message returned when the forgot-password reset session is invalid or expired.
        /// </summary>
        public const string RESET_SESSION_INVALID_MESSAGE = "Reset session is invalid or expired.";
    }

    /// <summary>
    /// Contains account and sign-in error messages.
    /// </summary>
    public static class AccountErrors
    {
        /// <summary>
        /// Message returned when a refresh token is invalid.
        /// </summary>
        public const string INVALID_REFRESH_TOKEN_MESSAGE = "Invalid refresh token.";

        /// <summary>
        /// Message returned when a refresh request reuses the immediate previous token.
        /// </summary>
        public const string REFRESH_TOKEN_DUPLICATE_MESSAGE = "Refresh token request was already processed.";

        /// <summary>
        /// Message returned when a token-issuing request does not include required client device metadata.
        /// </summary>
        public const string DEVICE_INFO_REQUIRED_MESSAGE = "Device info is required.";

        /// <summary>
        /// Message returned when the target account is inactive.
        /// </summary>
        public const string ACCOUNT_INACTIVE_MESSAGE = "Account is inactive.";

        /// <summary>
        /// Message returned when an account already exists and external linking must happen after sign-in.
        /// </summary>
        public const string ACCOUNT_ALREADY_EXISTS_LINK_MESSAGE = "Account already exists. Sign in and link the external provider.";

        /// <summary>
        /// Message returned when the requested username already exists.
        /// </summary>
        public const string USERNAME_ALREADY_EXISTS_MESSAGE = "Username already exists.";

        /// <summary>
        /// Message returned when the requested email already exists.
        /// </summary>
        public const string EMAIL_ALREADY_EXISTS_MESSAGE = "Email already exists.";

        /// <summary>
        /// Message returned when the requested phone number already exists.
        /// </summary>
        public const string PHONE_NUMBER_ALREADY_EXISTS_MESSAGE = "Phone number already exists.";

        /// <summary>
        /// Generic message returned for invalid login credentials.
        /// </summary>
        public const string INVALID_USERNAME_OR_PASSWORD_MESSAGE = "Invalid username or password.";

        /// <summary>
        /// Message returned when the external-login flow cannot load the target user.
        /// </summary>
        public const string EXTERNAL_USER_NOT_LOADED_MESSAGE = "External user was not loaded.";
    }

    /// <summary>
    /// Contains password-change error messages.
    /// </summary>
    public static class PasswordErrors
    {
        /// <summary>
        /// Message returned when the supplied current password is invalid.
        /// </summary>
        public const string CURRENT_PASSWORD_INVALID_MESSAGE = "Current password is invalid.";
    }

    /// <summary>
    /// Contains external identity provider error messages.
    /// </summary>
    public static class ExternalProviderErrors
    {
        /// <summary>
        /// Message returned when the requested external identity provider is invalid.
        /// </summary>
        public const string INVALID_EXTERNAL_PROVIDER_MESSAGE = "Invalid external provider.";

        /// <summary>
        /// Message returned when an external token cannot be validated.
        /// </summary>
        public const string INVALID_EXTERNAL_TOKEN_MESSAGE = "Invalid external token.";

        /// <summary>
        /// Message returned when the external identity provider is temporarily unavailable.
        /// </summary>
        public const string EXTERNAL_PROVIDER_UNAVAILABLE_MESSAGE = "External identity provider is temporarily unavailable.";

        /// <summary>
        /// Message returned when the external provider does not supply the required email.
        /// </summary>
        public const string EXTERNAL_EMAIL_REQUIRED_MESSAGE = "The external provider must return an email address.";

        /// <summary>
        /// Message returned when an external provider is already linked to another user.
        /// </summary>
        public const string EXTERNAL_PROVIDER_LINK_CONFLICT_MESSAGE = "External provider is already linked to another user.";

        /// <summary>
        /// Message returned when the current user has not linked the requested provider.
        /// </summary>
        public const string EXTERNAL_PROVIDER_NOT_LINKED_MESSAGE = "External provider is not linked to the current account.";

        /// <summary>
        /// Message returned when unlinking would remove the last usable sign-in method.
        /// </summary>
        public const string LAST_SIGN_IN_METHOD_REQUIRED_MESSAGE = "At least one sign-in method must remain linked to the account.";
    }

    /// <summary>
    /// Contains profile and change-email error messages.
    /// </summary>
    public static class ProfileErrors
    {
        /// <summary>
        /// Message returned when an uploaded avatar image is empty.
        /// </summary>
        public const string AVATAR_FILE_EMPTY_MESSAGE = "Avatar image file is empty.";

        /// <summary>
        /// Message returned when profile avatar upload fails.
        /// </summary>
        public const string AVATAR_UPLOAD_FAILED_MESSAGE = "Unable to upload avatar image at this time.";

        /// <summary>
        /// Message returned when user-info update cannot be persisted.
        /// </summary>
        public const string USER_INFO_UPDATE_FAILED_MESSAGE = "Unable to update user info at this time.";

        /// <summary>
        /// Message returned when change-email pending data has expired.
        /// </summary>
        public const string CHANGE_EMAIL_SESSION_EXPIRED_MESSAGE = "Change email session has expired.";

        /// <summary>
        /// Message returned when the requested gender value is unsupported.
        /// </summary>
        public const string INVALID_GENDER_MESSAGE = "Invalid gender.";
    }

    /// <summary>
    /// Contains KYC error messages.
    /// </summary>
    public static class KycErrors
    {
        /// <summary>
        /// Message returned when a KYC file is missing or empty.
        /// </summary>
        public const string KYC_FILE_REQUIRED_MESSAGE = "Required identity document file is missing.";

        /// <summary>
        /// Message returned when KYC submission cannot be persisted.
        /// </summary>
        public const string KYC_SUBMISSION_FAILED_MESSAGE = "Unable to submit KYC at this time.";

        /// <summary>
        /// Message returned when a KYC identifier is already owned by a different account context.
        /// </summary>
        public const string KYC_IDENTIFIER_ALREADY_USED_MESSAGE = "Identifier is already used by another account.";

        /// <summary>
        /// Message returned when the account already has a KYC submission waiting for review or approved.
        /// </summary>
        public const string KYC_REUPLOAD_NOT_ALLOWED_MESSAGE = "KYC is already submitted or approved. Re-upload is allowed only after rejection.";
    }
}
