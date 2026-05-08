namespace Be.Haven.Cache.Helpers;

/// <summary>
/// Builds standardized cache scopes for scoped cache entries.
/// </summary>
public static class CacheScopes
{
    /// <summary>
    /// Returns the sentinel scope used for requests whose cache scope should resolve to the current authenticated user.
    /// </summary>
    /// <returns>The current-user cache scope sentinel.</returns>
    public static string CurrentUser() => CURRENT_USER_CACHE_SCOPE;

    /// <summary>
    /// Builds the cache scope for a user-scoped cache entry.
    /// </summary>
    /// <param name="currentUserPublicId">The public identifier of the current authenticated user.</param>
    /// <returns>The normalized user scope string.</returns>
    public static string User(Guid currentUserPublicId) => string.Format(USER_CACHE_SCOPE_FORMAT, currentUserPublicId);
}
