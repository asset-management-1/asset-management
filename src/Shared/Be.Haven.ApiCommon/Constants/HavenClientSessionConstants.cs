namespace Be.Haven.ApiCommon.Constants;

/// <summary>
/// Contains constants used by shared Haven client-session validation.
/// </summary>
public static class HavenClientSessionConstants
{
    /// <summary>
    /// Identifies the master-data type that owns authentication account statuses.
    /// </summary>
    public const string USER_STATUS_TYPE_CODE = "UserStatus";

    /// <summary>
    /// Identifies the only user status allowed to keep an authenticated session active.
    /// </summary>
    public const string ACTIVE_USER_STATUS_CODE = "ACTIVE";

    /// <summary>
    /// Minimal PostgreSQL query that confirms a client session is active and belongs to the token user.
    /// </summary>
    public const string GET_ACTIVE_SESSION_BY_USER_AND_PUBLIC_ID_QUERY = """
        SELECT
            rt."SessionPublicId"
        FROM "identity"."RefreshTokens" rt
        INNER JOIN "identity"."Users" u
            ON u."Id" = rt."UserId"
        INNER JOIN "masterdata"."MasterDataValues" user_status
            ON user_status."Id" = u."StatusId"
           AND user_status."IsDeleted" = FALSE
           AND user_status."IsActive" = TRUE
           AND user_status."Code" = @ActiveUserStatusCode
        INNER JOIN "masterdata"."MasterDataTypes" user_status_type
            ON user_status_type."Id" = user_status."MasterDataTypeId"
           AND user_status_type."IsDeleted" = FALSE
           AND user_status_type."Code" = @UserStatusTypeCode
        WHERE rt."SessionPublicId" = @SessionPublicId
          AND u."PublicId" = @UserPublicId
          AND rt."RevokedAt" IS NULL
          AND rt."ExpiresAt" > CURRENT_TIMESTAMP
          AND rt."IsDeleted" = FALSE
          AND u."IsDeleted" = FALSE
        LIMIT 1;
        """;
}
