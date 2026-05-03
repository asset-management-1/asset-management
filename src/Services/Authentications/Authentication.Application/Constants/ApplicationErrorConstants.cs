namespace Authentication.Application.Constants;

public static class ApplicationErrorConstants
{
    public const string PASSWORD_COMPLEXITY_RULES = "Password must meet complexity rules (min 8 chars, include uppercase, lowercase, number, and special character).";

    public const string AUTH_INVALID_CREDENTIALS = "error_auth_invalid_credentials";

    public const string AUTH_UNAUTHORIZED = "error_unauthorized";

    public const string AUTH_USER_ALREADY_EXISTS = "error_auth_user_already_exists";

    public const string AUTH_USER_STATUS_NOT_FOUND = "error_auth_user_status_not_found";

    public const string AUTH_INVALID_REFRESH_TOKEN = "error_auth_invalid_refresh_token";

    public const string AUTH_FORBIDDEN_OPERATION = "error_auth_forbidden_operation";

    public const string AUTH_OTP_INVALID = "error_auth_otp_invalid";

    public const string AUTH_OTP_COOLDOWN = "error_auth_otp_cooldown";

    public const string AUTH_OTP_RATE_LIMIT = "error_auth_otp_rate_limit";

    public const string AUTH_PENDING_REGISTER_NOT_FOUND = "error_auth_pending_register_not_found";

    public const string AUTH_RESET_SESSION_INVALID = "error_auth_reset_session_invalid";

    public const string AUTH_EXTERNAL_PROVIDER_INVALID = "error_auth_external_provider_invalid";

    public const string AUTH_EXTERNAL_PROVIDER_LINK_CONFLICT = "error_auth_external_provider_link_conflict";

    public const string AUTH_EMAIL_NOT_VERIFIED = "error_auth_email_not_verified";

    public const string AUTH_ACCOUNT_INACTIVE = "error_auth_account_inactive";
}
