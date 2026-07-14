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
    /// Filter used for active rows with a non-null current party context.
    /// </summary>
    public const string CURRENT_PARTY_ID_FILTER = "\"CurrentPartyId\" IS NOT NULL AND \"IsDeleted\" = false";

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
}
