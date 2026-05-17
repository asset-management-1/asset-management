namespace Authentication.Application.Constants;

/// <summary>
/// Stores shared validation and application-flow messages used by the authentication application layer.
/// </summary>
public static class ApplicationConstants
{
    /// <summary>
    /// Logical cache group key used for the current-user profile query.
    /// </summary>
    public const string USER_INFO_CACHE_KEY = "auth:user-info";

    /// <summary>
    /// Logical cache group key used for the current tenant profile vehicles query.
    /// </summary>
    public const string USER_VEHICLES_CACHE_KEY = "auth:user-vehicles";

    /// <summary>
    /// Generic unauthorized-request message used by application handlers.
    /// </summary>
    public const string UNAUTHORIZED_REQUEST_MESSAGE = "Unauthorized request.";

    /// <summary>
    /// Master-data type code for user profile gender values.
    /// </summary>
    public const string GENDER_TYPE = "Gender";

    /// <summary>
    /// Validation message returned when a target context is unsupported.
    /// </summary>
    public const string INVALID_TARGET_CONTEXT_VALIDATION_MESSAGE = "TargetContext must be tenant or landlord.";

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
    /// Master-data code for the active status value.
    /// </summary>
    public const string ACTIVE_STATUS = "ACTIVE";

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
    /// Email subject used when sending register verification OTP emails.
    /// </summary>
    public const string EMAIL_SUBJECT_VERIFY_ACCOUNT = "Verify your Haven account";

    /// <summary>
    /// Email subject used when sending forgot-password OTP emails.
    /// </summary>
    public const string EMAIL_SUBJECT_RESET_PASSWORD = "Reset your Haven password";

    /// <summary>
    /// Email subject used when sending change-email OTP emails to the new address.
    /// </summary>
    public const string EMAIL_SUBJECT_CHANGE_EMAIL = "Verify your new Haven email";

    /// <summary>
    /// Email subject used when notifying the previous address about a change-email request.
    /// </summary>
    public const string EMAIL_SUBJECT_CHANGE_EMAIL_SECURITY = "Security notice: email change requested";

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
