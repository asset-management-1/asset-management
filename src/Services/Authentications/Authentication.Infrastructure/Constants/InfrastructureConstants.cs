namespace Authentication.Infrastructure.Constants;

/// <summary>
/// Stores infrastructure-layer constants used by authentication workflows.
/// </summary>
public static class InfrastructureConstants
{
    /// <summary>
    /// OTP purpose code used by the register flow.
    /// </summary>
    public const string REGISTER_PURPOSE = "register";

    /// <summary>
    /// OTP purpose code used by the forgot-password flow.
    /// </summary>
    public const string FORGOT_PASSWORD_PURPOSE = "forgot-password";

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
    /// Master-data type code for user profile and document gender values.
    /// </summary>
    public const string PROFILE_GENDER_TYPE = "Gender";

    /// <summary>
    /// Master-data type code for tenant profile vehicle types.
    /// </summary>
    public const string VEHICLE_TYPE_TYPE = "VehicleType";

    /// <summary>
    /// Master-data type code for identifier type values.
    /// </summary>
    public const string IDENTIFIER_TYPE_TYPE = "IdentifierType";

    /// <summary>
    /// Master-data type code for document type values.
    /// </summary>
    public const string DOCUMENT_TYPE_TYPE = "DocumentType";

    /// <summary>
    /// Master-data type code for storage provider values.
    /// </summary>
    public const string STORAGE_PROVIDER_TYPE = "StorageProvider";

    /// <summary>
    /// Master-data type code for document status values.
    /// </summary>
    public const string DOCUMENT_STATUS_TYPE = "DocumentStatus";

    /// <summary>
    /// Master-data type code for generic linked entity values.
    /// </summary>
    public const string ENTITY_TYPE_TYPE = "EntityType";

    /// <summary>
    /// Master-data type code for document-link type values.
    /// </summary>
    public const string DOCUMENT_LINK_TYPE_TYPE = "DocumentLinkType";

    /// <summary>
    /// Master-data type code for document-link status values.
    /// </summary>
    public const string DOCUMENT_LINK_STATUS_TYPE = "DocumentLinkStatus";

    /// <summary>
    /// Master-data type code for manual identity-document KYC review status values.
    /// </summary>
    public const string KYC_STATUS_TYPE = "KycStatus";

    /// <summary>
    /// Master-data code for national-id document metadata.
    /// </summary>
    public const string NATIONAL_ID_DOCUMENT_TYPE = "NATIONAL_ID";

    /// <summary>
    /// Master-data code for Cloudflare R2 private storage.
    /// </summary>
    public const string R2_STORAGE_PROVIDER = "R2";

    /// <summary>
    /// Master-data code for uploaded document status.
    /// </summary>
    public const string UPLOADED_DOCUMENT_STATUS = "UPLOADED";

    /// <summary>
    /// Master-data code for party entity document links.
    /// </summary>
    public const string PARTY_ENTITY_TYPE = "PARTY";

    /// <summary>
    /// Master-data code for single-file national-id scans.
    /// </summary>
    public const string NATIONAL_ID_SCAN_LINK_TYPE = "NATIONAL_ID_SCAN";

    /// <summary>
    /// Master-data code for front-side national-id scans.
    /// </summary>
    public const string NATIONAL_ID_FRONT_SCAN_LINK_TYPE = "NATIONAL_ID_FRONT_SCAN";

    /// <summary>
    /// Master-data code for back-side national-id scans.
    /// </summary>
    public const string NATIONAL_ID_BACK_SCAN_LINK_TYPE = "NATIONAL_ID_BACK_SCAN";

    /// <summary>
    /// Master-data code for active document-link status.
    /// </summary>
    public const string ACTIVE_DOCUMENT_LINK_STATUS = "ACTIVE";

    /// <summary>
    /// Fallback display-name suffix appended when a party context is auto-created.
    /// </summary>
    public const string PARTY_CONTEXT_DISPLAY_NAME_SUFFIX_FORMAT = "{0} ({1})";

    /// <summary>
    /// Error message used when a party context value cannot be mapped to the public enum contract.
    /// </summary>
    public const string UNSUPPORTED_PARTY_CONTEXT_VALUE_MESSAGE = "Unsupported party context value '{0}'.";

    /// <summary>
    /// Default username prefix used for externally provisioned users.
    /// </summary>
    public const string DEFAULT_EXTERNAL_USER_NAME = "user";

    /// <summary>
    /// Sort order used for the front-side or primary identity document link.
    /// </summary>
    public const int KYC_FRONT_DOCUMENT_SORT_ORDER = 1;

    /// <summary>
    /// Sort order used for the back-side identity document link.
    /// </summary>
    public const int KYC_BACK_DOCUMENT_SORT_ORDER = 2;

    /// <summary>
    /// Token type returned for JWT bearer authentication.
    /// </summary>
    public const string TOKEN_TYPE_BEARER = "Bearer";

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
    /// Time-to-live, in minutes, for OTP cache entries.
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
    /// Sentinel cache value used to mark an active OTP cooldown entry.
    /// </summary>
    public const string OTP_COOLDOWN_VALUE = "1";

    /// <summary>
    /// Email subject used when sending register verification OTP emails.
    /// </summary>
    public const string EMAIL_SUBJECT_VERIFY_ACCOUNT = "Verify your Haven account";

    /// <summary>
    /// Email subject used when sending forgot-password OTP emails.
    /// </summary>
    public const string EMAIL_SUBJECT_RESET_PASSWORD = "Reset your Haven password";

    /// <summary>
    /// Plain-text email template used for OTP delivery.
    /// </summary>
    public const string OTP_EMAIL_TEXT_TEMPLATE = "Your Haven OTP is {0}. It expires in 5 minutes.";

    /// <summary>
    /// HTML email template used for OTP delivery.
    /// </summary>
    public const string OTP_EMAIL_HTML_TEMPLATE = "<p>Your Haven OTP is <strong>{0}</strong>. It expires in 5 minutes.</p>";

    /// <summary>
    /// HTML email template used for old-email security notification during change-email flow.
    /// </summary>
    public const string CHANGE_EMAIL_SECURITY_EMAIL_HTML_TEMPLATE =
        "<p>A request was made to change your Haven account email from <strong>{0}</strong> to <strong>{1}</strong>.</p><p>If this was not you, contact Haven support immediately at <strong>{2}</strong>.</p>";

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
}
