namespace Authentication.Application.Constants;

/// <summary>
/// Stores application-layer error codes and validation messages for authentication flows.
/// </summary>
public static class ApplicationErrorConstants
{
    /// <summary>
    /// Validation message returned when a password does not satisfy complexity rules.
    /// </summary>
    public const string PASSWORD_COMPLEXITY_RULES = "Password must meet complexity rules (min 8 chars, include uppercase, lowercase, number, and special character).";

    /// <summary>
    /// Validation message returned when the new password equals the current password.
    /// </summary>
    public const string NEW_PASSWORD_MUST_DIFFER_FROM_CURRENT_PASSWORD = "NewPassword must be different from CurrentPassword.";

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
    /// Error code returned when a refresh token is invalid, expired, or revoked.
    /// </summary>
    public const string AUTH_INVALID_REFRESH_TOKEN = "error_auth_invalid_refresh_token";

    /// <summary>
    /// Error code returned when token issuance cannot capture required client device metadata.
    /// </summary>
    public const string AUTH_DEVICE_INFO_REQUIRED = "error_auth_device_info_required";

    /// <summary>
    /// Error code returned when the requested authentication operation is forbidden.
    /// </summary>
    public const string AUTH_FORBIDDEN_OPERATION = "error_auth_forbidden_operation";

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
    /// Error code returned when the requested party context is invalid.
    /// </summary>
    public const string AUTH_INVALID_CONTEXT = "error_auth_invalid_context";

    /// <summary>
    /// Error code returned when profile input contains an unsupported gender value.
    /// </summary>
    public const string AUTH_INVALID_GENDER = "error_auth_invalid_gender";

    /// <summary>
    /// Error code returned when KYC submission data is invalid.
    /// </summary>
    public const string AUTH_KYC_INVALID = "error_auth_kyc_invalid";

    /// <summary>
    /// Error code returned when private KYC document upload fails.
    /// </summary>
    public const string AUTH_KYC_UPLOAD_FAILED = "error_auth_kyc_upload_failed";

    /// <summary>
    /// Error code returned when tenant vehicle input or state is invalid.
    /// </summary>
    public const string AUTH_VEHICLE_INVALID = "error_auth_vehicle_invalid";

    /// <summary>
    /// Error code returned when profile avatar upload fails.
    /// </summary>
    public const string AUTH_PROFILE_UPLOAD_FAILED = "error_auth_profile_upload_failed";

    /// <summary>
    /// Error code returned when profile update persistence fails after upload work succeeds.
    /// </summary>
    public const string AUTH_PROFILE_UPDATE_FAILED = "error_auth_profile_update_failed";

    /// <summary>
    /// Error code returned when a tenant vehicle cannot be found.
    /// </summary>
    public const string AUTH_VEHICLE_NOT_FOUND = "error_auth_vehicle_not_found";

    /// <summary>
    /// Error code returned when a tenant vehicle image upload fails.
    /// </summary>
    public const string AUTH_VEHICLE_UPLOAD_FAILED = "error_auth_vehicle_upload_failed";

    /// <summary>
    /// Error code returned when the user's email address has not been verified.
    /// </summary>
    public const string AUTH_EMAIL_NOT_VERIFIED = "error_auth_email_not_verified";

    /// <summary>
    /// Error code returned when the target account is inactive.
    /// </summary>
    public const string AUTH_ACCOUNT_INACTIVE = "error_auth_account_inactive";

    /// <summary>
    /// Message returned when the requested party type is invalid.
    /// </summary>
    public const string INVALID_PARTY_TYPE_MESSAGE = "Invalid party type.";

    /// <summary>
    /// Message returned when OTP delivery fails.
    /// </summary>
    public const string OTP_SEND_FAILED_MESSAGE = "Unable to send OTP at this time.";

    /// <summary>
    /// Message returned when pending registration data has expired.
    /// </summary>
    public const string REGISTRATION_SESSION_EXPIRED_MESSAGE = "Registration session has expired.";

    /// <summary>
    /// Message returned when required master data cannot be found.
    /// </summary>
    public const string REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE = "Required master data was not found.";

    /// <summary>
    /// Message returned when a refresh token is invalid.
    /// </summary>
    public const string INVALID_REFRESH_TOKEN_MESSAGE = "Invalid refresh token.";

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
    /// Generic success message returned by forgot-password OTP requests.
    /// </summary>
    public const string FORGOT_PASSWORD_SUCCESS_MESSAGE = "If the account exists, an OTP has been sent.";

    /// <summary>
    /// Success message returned after OTP verification.
    /// </summary>
    public const string OTP_VERIFIED_SUCCESS_MESSAGE = "OTP verified successfully.";

    /// <summary>
    /// Message returned when the forgot-password reset session is invalid or expired.
    /// </summary>
    public const string RESET_SESSION_INVALID_MESSAGE = "Reset session is invalid or expired.";

    /// <summary>
    /// Success message returned after a password change.
    /// </summary>
    public const string PASSWORD_CHANGED_SUCCESS_MESSAGE = "Password changed successfully.";

    /// <summary>
    /// Success message returned after logout.
    /// </summary>
    public const string LOGOUT_SUCCESS_MESSAGE = "Logged out successfully.";

    /// <summary>
    /// Success message returned after linking an external provider.
    /// </summary>
    public const string EXTERNAL_PROVIDER_LINKED_SUCCESS_MESSAGE = "External provider linked successfully.";

    /// <summary>
    /// Success message returned after unlinking an external provider.
    /// </summary>
    public const string EXTERNAL_PROVIDER_UNLINKED_SUCCESS_MESSAGE = "External provider unlinked successfully.";

    /// <summary>
    /// Message returned when the supplied current password is invalid.
    /// </summary>
    public const string CURRENT_PASSWORD_INVALID_MESSAGE = "Current password is invalid.";

    /// <summary>
    /// Message returned when the requested external identity provider is invalid.
    /// </summary>
    public const string INVALID_EXTERNAL_PROVIDER_MESSAGE = "Invalid external provider.";

    /// <summary>
    /// Message returned when an external token cannot be validated.
    /// </summary>
    public const string INVALID_EXTERNAL_TOKEN_MESSAGE = "Invalid external token.";

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

    /// <summary>
    /// Message returned when a target party context is invalid.
    /// </summary>
    public const string INVALID_TARGET_CONTEXT_MESSAGE = "Invalid target context.";

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

    /// <summary>
    /// Message returned when the active status master-data value cannot be found.
    /// </summary>
    public const string ACTIVE_STATUS_NOT_FOUND_MESSAGE = "Active status was not found.";

    /// <summary>
    /// Message returned when the default external-user party type cannot be found.
    /// </summary>
    public const string DEFAULT_PARTY_TYPE_NOT_FOUND_MESSAGE = "Default party type was not found.";

    /// <summary>
    /// Success message returned after an OTP is sent.
    /// </summary>
    public const string OTP_SENT_MESSAGE = "OTP sent";

    /// <summary>
    /// Success message returned after registration is completed.
    /// </summary>
    public const string REGISTRATION_COMPLETED_SUCCESS_MESSAGE = "Registration completed successfully.";

    /// <summary>
    /// Success message returned after switching party context.
    /// </summary>
    public const string SWITCH_PARTY_SUCCESS_MESSAGE = "Party context switched successfully.";

    /// <summary>
    /// Success message returned after current-user profile fields are updated.
    /// </summary>
    public const string USER_INFO_UPDATED_SUCCESS_MESSAGE = "User info updated successfully.";

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
    /// Success message returned after sending change-email OTP.
    /// </summary>
    public const string CHANGE_EMAIL_OTP_SENT_MESSAGE = "Change email OTP sent.";

    /// <summary>
    /// Success message returned after verifying and applying a new email.
    /// </summary>
    public const string CHANGE_EMAIL_COMPLETED_SUCCESS_MESSAGE = "Email changed successfully.";

    /// <summary>
    /// Message returned when change-email pending data has expired.
    /// </summary>
    public const string CHANGE_EMAIL_SESSION_EXPIRED_MESSAGE = "Change email session has expired.";

    /// <summary>
    /// Message returned when the requested gender value is unsupported.
    /// </summary>
    public const string INVALID_GENDER_MESSAGE = "Invalid gender.";

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
    public const string KYC_REUPLOAD_NOT_ALLOWED_MESSAGE =
        "KYC is already submitted or approved. Re-upload is allowed only after rejection.";

    /// <summary>
    /// Success message returned after a KYC submission is accepted for review.
    /// </summary>
    public const string KYC_SUBMITTED_SUCCESS_MESSAGE = "KYC submitted for review.";

    /// <summary>
    /// Message returned when the active user context is not tenant.
    /// </summary>
    public const string TENANT_CONTEXT_REQUIRED_MESSAGE = "Tenant context is required.";

    /// <summary>
    /// Message returned when a tenant vehicle type cannot be resolved.
    /// </summary>
    public const string INVALID_VEHICLE_TYPE_MESSAGE = "Invalid vehicle type.";

    /// <summary>
    /// Message returned when a vehicle type code cannot be mapped to the public enum contract.
    /// </summary>
    public const string UNSUPPORTED_VEHICLE_TYPE_VALUE_MESSAGE = "Unsupported vehicle type value '{0}'.";

    /// <summary>
    /// Message returned when a vehicle status code cannot be mapped to the public enum contract.
    /// </summary>
    public const string UNSUPPORTED_VEHICLE_STATUS_VALUE_MESSAGE = "Unsupported vehicle status value '{0}'.";

    /// <summary>
    /// Message returned when a tenant vehicle cannot be found.
    /// </summary>
    public const string VEHICLE_NOT_FOUND_MESSAGE = "Vehicle was not found.";

    /// <summary>
    /// Message returned when a tenant vehicle image upload fails.
    /// </summary>
    public const string VEHICLE_UPLOAD_FAILED_MESSAGE = "Unable to upload vehicle image at this time.";

    /// <summary>
    /// Message returned when a tenant vehicle cannot be registered after upload or validation succeeds.
    /// </summary>
    public const string VEHICLE_REGISTRATION_FAILED_MESSAGE = "Unable to register vehicle at this time.";

    /// <summary>
    /// Message returned when an uploaded tenant vehicle image is empty.
    /// </summary>
    public const string VEHICLE_FILE_EMPTY_MESSAGE = "Vehicle image file is empty.";

    /// <summary>
    /// Success message returned after a tenant vehicle is removed.
    /// </summary>
    public const string VEHICLE_DELETED_SUCCESS_MESSAGE = "Vehicle deleted successfully.";
}
