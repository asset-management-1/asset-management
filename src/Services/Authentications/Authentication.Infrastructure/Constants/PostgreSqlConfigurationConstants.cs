namespace Authentication.Infrastructure.Constants;

/// <summary>
/// Defines PostgreSQL-specific SQL fragments used by EF entity configurations in the authentication module.
/// </summary>
internal static class PostgreSqlConfigurationConstants
{
    /// <summary>
    /// Default SQL used to populate UTC creation timestamps.
    /// </summary>
    public const string CURRENT_TIMESTAMP_SQL = "CURRENT_TIMESTAMP";

    /// <summary>
    /// Default SQL used to generate UUID values for public identifiers.
    /// </summary>
    public const string GENERATED_UUID_SQL = "gen_random_uuid()";

    /// <summary>
    /// PostgreSQL column type used for date-only fields.
    /// </summary>
    public const string DATE_COLUMN_TYPE = "date";

    /// <summary>
    /// Filter used for rows that are not soft deleted.
    /// </summary>
    public const string NOT_DELETED_FILTER = "\"IsDeleted\" = false";

    /// <summary>
    /// Filter used for active rows with a non-null profile gender.
    /// </summary>
    public const string GENDER_ID_FILTER = "\"GenderId\" IS NOT NULL AND \"IsDeleted\" = false";

    /// <summary>
    /// Filter used for active rows with a non-null primary email.
    /// </summary>
    public const string PRIMARY_EMAIL_FILTER = "\"PrimaryEmail\" IS NOT NULL AND \"IsDeleted\" = false";

    /// <summary>
    /// Filter used for active rows with a non-null primary phone.
    /// </summary>
    public const string PRIMARY_PHONE_FILTER = "\"PrimaryPhone\" IS NOT NULL AND \"IsDeleted\" = false";

    /// <summary>
    /// Filter used for active rows with a non-null email.
    /// </summary>
    public const string EMAIL_FILTER = "\"Email\" IS NOT NULL AND \"IsDeleted\" = false";

    /// <summary>
    /// Filter used for active rows with a non-null phone number.
    /// </summary>
    public const string PHONE_NUMBER_FILTER = "\"PhoneNumber\" IS NOT NULL AND \"IsDeleted\" = false";

    /// <summary>
    /// Filter used for active rows with a non-null user name.
    /// </summary>
    public const string USER_NAME_FILTER = "\"UserName\" IS NOT NULL AND \"IsDeleted\" = false";

    /// <summary>
    /// Filter used for refresh tokens with a client instance id.
    /// </summary>
    public const string REFRESH_TOKEN_DEVICE_FILTER = "\"DeviceId\" IS NOT NULL AND \"IsDeleted\" = false";

    /// <summary>
    /// Filter used for refresh tokens that carry a server-issued session id.
    /// </summary>
    public const string REFRESH_TOKEN_SESSION_PUBLIC_ID_FILTER = "\"SessionPublicId\" IS NOT NULL AND \"IsDeleted\" = false";

    /// <summary>
    /// Unique index that protects canonical usernames.
    /// </summary>
    public const string USER_NAME_UNIQUE_CONSTRAINT = "UX_Identity_Users_UserName";

    /// <summary>
    /// Unique index that protects canonical email addresses.
    /// </summary>
    public const string USER_EMAIL_UNIQUE_CONSTRAINT = "UX_Identity_Users_Email";

    /// <summary>
    /// Unique index that protects phone numbers supplied by clients.
    /// </summary>
    public const string USER_PHONE_UNIQUE_CONSTRAINT = "UX_Identity_Users_Phone";

    /// <summary>
    /// Unique constraint that prevents one external identity from linking to multiple users.
    /// </summary>
    public const string EXTERNAL_PROVIDER_KEY_UNIQUE_CONSTRAINT = "UQ_Identity_ExternalLogins_Provider_Key";

    /// <summary>
    /// Unique constraint that limits one provider link per user.
    /// </summary>
    public const string EXTERNAL_USER_PROVIDER_UNIQUE_CONSTRAINT = "UQ_Identity_ExternalLogins_User_Provider";
}
