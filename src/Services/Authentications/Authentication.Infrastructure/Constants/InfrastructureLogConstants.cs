namespace Authentication.Infrastructure.Constants;

public static class InfrastructureLogConstants
{
    public const string LOG_REGISTER_OTP_SENT = "Register OTP sent.";

    public const string LOG_REGISTER_COMPLETED = "User {UserPublicId} completed register email verification.";

    public const string LOG_LOGOUT_ALL_REVOKED = "All refresh tokens revoked for user {UserPublicId}.";

    public const string LOG_LOGOUT_REFRESH_TOKEN_REVOKED = "Refresh token revoked for user {UserPublicId}.";

    public const string LOG_FORGOT_PASSWORD_OTP_SENT = "Forgot-password OTP sent.";

    public const string LOG_FORGOT_PASSWORD_OTP_SEND_FAILED = "Forgot-password OTP email send failed.";

    public const string LOG_FORGOT_PASSWORD_OTP_VERIFIED = "Forgot-password OTP verified.";

    public const string LOG_FORGOT_PASSWORD_CHANGED = "Forgot-password change completed for user {UserPublicId}.";

    public const string LOG_PASSWORD_CHANGED = "Password changed for user {UserPublicId}.";

    public const string LOG_EXTERNAL_PROVIDER_LINKED = "External provider {Provider} linked for user {UserPublicId}.";

    public const string LOG_SENDGRID_CONFIGURATION_MISSING = "SendGrid configuration is missing for auth purpose {Purpose}.";

    public const string LOG_SENDGRID_OTP_SEND_FAILED = "OTP email send failed for auth purpose {Purpose}.";

    public const string LOG_EXTERNAL_TOKEN_VALIDATION_FAILED = "External token validation failed for provider {Provider}.";
}
