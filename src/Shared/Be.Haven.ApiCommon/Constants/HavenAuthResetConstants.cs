namespace Be.Haven.ApiCommon.Constants;

/// <summary>
/// Contains constants used by shared Haven authentication reset validation.
/// </summary>
public static class HavenAuthResetConstants
{
    /// <summary>
    /// Minimal PostgreSQL query that loads a user's auth reset timestamp by public identifier.
    /// </summary>
    public const string GET_AUTH_RESET_AT_BY_PUBLIC_ID_QUERY = """
        SELECT
            u."AuthResetAt"
        FROM "identity"."Users" u
        WHERE u."PublicId" = @UserPublicId
          AND u."IsDeleted" = FALSE
        LIMIT 1;
        """;
}
