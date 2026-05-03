namespace Authentication.Infrastructure.Constants;

public static class InfrastructureConstants
{
    public const string REGISTER_PURPOSE = "register";

    public const string FORGOT_PASSWORD_PURPOSE = "forgot-password";

    public const string ACTIVE_STATUS = "active";

    public const string STATUS_TYPE = "status";

    public const string PARTY_TYPE_TYPE = "partytype";

    public const string DEFAULT_EXTERNAL_USER_NAME = "user";

    public const string TOKEN_TYPE_BEARER = "Bearer";

    public const int OTP_LENGTH = 6;

    public const int OTP_MAX_VERIFY_ATTEMPTS = 5;

    public const int REGISTER_OTP_LIMIT = 10;

    public const int FORGOT_PASSWORD_OTP_LIMIT = 3;

    public const int OTP_TTL_MINUTES = 5;

    public const int OTP_COOLDOWN_SECONDS = 60;

    public const int OTP_LIMIT_TTL_MINUTES = 10;

    public const int RESET_SESSION_TTL_MINUTES = 5;

    public const string OTP_COOLDOWN_VALUE = "1";

    public const string EMAIL_SUBJECT_VERIFY_ACCOUNT = "Verify your Haven account";

    public const string EMAIL_SUBJECT_RESET_PASSWORD = "Reset your Haven password";

    public const string OTP_EMAIL_TEXT_TEMPLATE = "Your Haven OTP is {0}. It expires in 5 minutes.";

    public const string OTP_EMAIL_HTML_TEMPLATE = "<p>Your Haven OTP is <strong>{0}</strong>. It expires in 5 minutes.</p>";

    public const string OTP_KEY_PATTERN = "auth:otp:{0}:{1}";

    public const string PENDING_REGISTER_KEY_PATTERN = "auth:pending-register:{0}";

    public const string OTP_COOLDOWN_KEY_PATTERN = "auth:otp:cooldown:{0}:{1}";

    public const string OTP_LIMIT_KEY_PATTERN = "auth:otp:limit:{0}:{1}";

    public const string RESET_SESSION_KEY_PATTERN = "auth:reset-session:{0}";
}
