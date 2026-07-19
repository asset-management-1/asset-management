namespace Authentication.Application.Constants;

/// <summary>
/// Stores shared validation and application-flow messages used by the authentication application layer.
/// </summary>
public static class ApplicationConstants
{
    /// <summary>
    /// Minimum accepted password length for authentication password validators.
    /// </summary>
    public const int PASSWORD_MINIMUM_LENGTH = 8;

    /// <summary>
    /// Regular expression that requires at least one uppercase Latin letter in a password.
    /// </summary>
    public const string PASSWORD_UPPERCASE_PATTERN = "[A-Z]";

    /// <summary>
    /// Regular expression that requires at least one lowercase Latin letter in a password.
    /// </summary>
    public const string PASSWORD_LOWERCASE_PATTERN = "[a-z]";

    /// <summary>
    /// Regular expression that requires at least one numeric digit in a password.
    /// </summary>
    public const string PASSWORD_NUMBER_PATTERN = "[0-9]";

    /// <summary>
    /// Regular expression that requires at least one supported special character in a password.
    /// </summary>
    public const string PASSWORD_SPECIAL_CHARACTER_PATTERN = "[@$!%*?&]";

    /// <summary>
    /// OTP purpose code used by the register flow.
    /// </summary>
    public const string REGISTER_PURPOSE = "register";

    /// <summary>
    /// OTP purpose code used by the forgot-password flow.
    /// </summary>
    public const string FORGOT_PASSWORD_PURPOSE = "forgot-password";

    /// <summary>
    /// OTP purpose code used by the change-email flow.
    /// </summary>
    public const string CHANGE_EMAIL_PURPOSE = "change-email";

    /// <summary>
    /// Master-data type code for user-account status values.
    /// </summary>
    public const string USER_STATUS_TYPE = "UserStatus";

    /// <summary>
    /// Master-data type code for party status values.
    /// </summary>
    public const string PARTY_STATUS_TYPE = "PartyStatus";

    /// <summary>
    /// Master-data type code for party-type values.
    /// </summary>
    public const string PARTY_TYPE_TYPE = "PartyType";

    /// <summary>
    /// Fallback display-name suffix appended when a party context is auto-created.
    /// </summary>
    public const string PARTY_CONTEXT_DISPLAY_NAME_SUFFIX_FORMAT = "{0} ({1})";

    /// <summary>
    /// Default username prefix used for externally provisioned users.
    /// </summary>
    public const string DEFAULT_EXTERNAL_USER_NAME = "user";

    /// <summary>
    /// Canonical provider code for Google external authentication.
    /// </summary>
    public const string EXTERNAL_PROVIDER_GOOGLE = "google";

    /// <summary>
    /// Canonical provider code for Facebook external authentication.
    /// </summary>
    public const string EXTERNAL_PROVIDER_FACEBOOK = "facebook";

    /// <summary>
    /// Number of digits generated for each OTP code.
    /// </summary>
    public const int OTP_LENGTH = 4;

    /// <summary>
    /// Maximum number of invalid OTP verification attempts before the OTP is discarded.
    /// </summary>
    public const int OTP_MAX_VERIFY_ATTEMPTS = 5;

    /// <summary>
    /// Maximum number of register OTP requests allowed within the rate-limit window.
    /// </summary>
    public const int REGISTER_OTP_LIMIT = 10;

    /// <summary>
    /// Maximum number of forgot-password OTP requests allowed within the rate-limit window.
    /// </summary>
    public const int FORGOT_PASSWORD_OTP_LIMIT = 3;

    /// <summary>
    /// Time-to-live, in minutes, for OTP and short-lived auth session cache entries.
    /// </summary>
    public const int OTP_TTL_MINUTES = 5;

    /// <summary>
    /// Cooldown duration, in seconds, between OTP send attempts.
    /// </summary>
    public const int OTP_COOLDOWN_SECONDS = 60;

    /// <summary>
    /// Time-to-live, in minutes, for OTP request rate-limit counters.
    /// </summary>
    public const int OTP_LIMIT_TTL_MINUTES = 10;

    /// <summary>
    /// Time-to-live, in minutes, for forgot-password reset sessions.
    /// </summary>
    public const int RESET_SESSION_TTL_MINUTES = 5;

    /// <summary>
    /// Token type returned for JWT bearer authentication.
    /// </summary>
    public const string TOKEN_TYPE_BEARER = "Bearer";

    /// <summary>
    /// Sentinel cache value used to mark an active OTP cooldown entry.
    /// </summary>
    public const string OTP_COOLDOWN_VALUE = "1";

    /// <summary>
    /// Redis key pattern for OTP cache entries.
    /// </summary>
    public const string OTP_KEY_PATTERN = "auth:otp:{0}:{1}";

    /// <summary>
    /// Redis key pattern for register session cache entries.
    /// </summary>
    public const string REGISTER_SESSION_KEY_PATTERN = "auth:register-session:{0}";

    /// <summary>
    /// Redis key pattern for OTP cooldown markers.
    /// </summary>
    public const string OTP_COOLDOWN_KEY_PATTERN = "auth:otp:cooldown:{0}:{1}";

    /// <summary>
    /// Redis key pattern for OTP rate-limit counters.
    /// </summary>
    public const string OTP_LIMIT_KEY_PATTERN = "auth:otp:limit:{0}:{1}";

    /// <summary>
    /// Cache-key format for atomic OTP verification-attempt counters.
    /// </summary>
    public const string OTP_VERIFY_ATTEMPT_KEY_PATTERN = "{0}:verify-attempts";

    /// <summary>
    /// Cache-key pattern that allows one successful consumer to complete an OTP flow.
    /// </summary>
    public const string OTP_CONSUME_KEY_PATTERN = "{0}:consume";

    /// <summary>
    /// Redis key pattern for forgot-password reset sessions.
    /// </summary>
    public const string RESET_SESSION_KEY_PATTERN = "auth:reset-session:{0}";

    /// <summary>
    /// Redis key pattern for change-email session cache entries.
    /// </summary>
    public const string CHANGE_EMAIL_SESSION_KEY_PATTERN = "auth:change-email-session:{0}";

    /// <summary>
    /// Redis key pattern for change-email OTP cooldown markers.
    /// </summary>
    public const string CHANGE_EMAIL_COOLDOWN_KEY_PATTERN = "auth:otp:cooldown:change-email:{0}";
}
