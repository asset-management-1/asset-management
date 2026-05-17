namespace Be.Haven.ApiCommon.Constants;

/// <summary>
/// Contains constants used by shared Haven client-session validation.
/// </summary>
public static class HavenClientSessionConstants
{
    /// <summary>
    /// Minimal PostgreSQL query that confirms a client session is active and belongs to the token user.
    /// </summary>
    public const string GET_ACTIVE_SESSION_BY_USER_AND_PUBLIC_ID_QUERY = """
        SELECT
            rt."SessionPublicId"
        FROM "identity"."RefreshTokens" rt
        INNER JOIN "identity"."Users" u
            ON u."Id" = rt."UserId"
        WHERE rt."SessionPublicId" = @SessionPublicId
          AND u."PublicId" = @UserPublicId
          AND rt."RevokedAt" IS NULL
          AND rt."ExpiresAt" > CURRENT_TIMESTAMP
          AND rt."IsDeleted" = FALSE
          AND u."IsDeleted" = FALSE
        LIMIT 1;
        """;
}
