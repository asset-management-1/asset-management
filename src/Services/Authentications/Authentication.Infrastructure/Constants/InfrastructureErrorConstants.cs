namespace Authentication.Infrastructure.Constants;

public static class InfrastructureErrorConstants
{
    public const string INVALID_PARTY_TYPE_MESSAGE = "Invalid party type.";

    public const string OTP_SEND_FAILED_MESSAGE = "Unable to send OTP at this time.";

    public const string REGISTRATION_SESSION_EXPIRED_MESSAGE = "Registration session has expired.";

    public const string REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE = "Required master data was not found.";

    public const string DEFAULT_ROLE_NOT_FOUND_MESSAGE = "Default role was not found.";

    public const string INVALID_REFRESH_TOKEN_MESSAGE = "Invalid refresh token.";

    public const string ACCOUNT_INACTIVE_MESSAGE = "Account is inactive.";

    public const string ACCOUNT_ALREADY_EXISTS_LINK_MESSAGE = "Account already exists. Sign in and link the external provider.";

    public const string FORGOT_PASSWORD_SUCCESS_MESSAGE = "If the account exists, an OTP has been sent.";

    public const string OTP_VERIFIED_SUCCESS_MESSAGE = "OTP verified successfully.";

    public const string RESET_SESSION_INVALID_MESSAGE = "Reset session is invalid or expired.";

    public const string PASSWORD_CHANGED_SUCCESS_MESSAGE = "Password changed successfully.";

    public const string LOGOUT_SUCCESS_MESSAGE = "Logged out successfully.";

    public const string EXTERNAL_PROVIDER_LINKED_SUCCESS_MESSAGE = "External provider linked successfully.";

    public const string UNAUTHORIZED_REQUEST_MESSAGE = "Unauthorized request.";

    public const string CURRENT_PASSWORD_INVALID_MESSAGE = "Current password is invalid.";

    public const string INVALID_EXTERNAL_PROVIDER_MESSAGE = "Invalid external provider.";

    public const string INVALID_EXTERNAL_TOKEN_MESSAGE = "Invalid external token.";

    public const string EXTERNAL_PROVIDER_LINK_CONFLICT_MESSAGE = "External provider is already linked to another user.";

    public const string OTP_INVALID_OR_EXPIRED_MESSAGE = "OTP is invalid or expired.";

    public const string OTP_COOLDOWN_MESSAGE = "Please wait before requesting another OTP.";

    public const string OTP_RATE_LIMIT_MESSAGE = "Too many OTP requests. Please try again later.";

    public const string USERNAME_ALREADY_EXISTS_MESSAGE = "Username already exists.";

    public const string EMAIL_ALREADY_EXISTS_MESSAGE = "Email already exists.";

    public const string PHONE_NUMBER_ALREADY_EXISTS_MESSAGE = "Phone number already exists.";

    public const string INVALID_USERNAME_OR_PASSWORD_MESSAGE = "Invalid username or password.";

    public const string EXTERNAL_USER_NOT_LOADED_MESSAGE = "External user was not loaded.";

    public const string ACTIVE_STATUS_NOT_FOUND_MESSAGE = "Active status was not found.";

    public const string DEFAULT_PARTY_TYPE_NOT_FOUND_MESSAGE = "Default party type was not found.";

    public const string OTP_SENT_MESSAGE = "OTP sent";

    public const string REGISTRATION_COMPLETED_SUCCESS_MESSAGE = "Registration completed successfully.";
}
