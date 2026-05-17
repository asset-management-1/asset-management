namespace Be.Haven.Cache.Extensions;

/// <summary>
/// Provides cache-scope formatting helpers for scoped cache entries.
/// </summary>
public static class CacheScopeExtension
{
    /// <summary>
    /// Converts a user public identifier into the standardized user cache scope.
    /// </summary>
    /// <param name="currentUserPublicId">The public identifier of the current authenticated user.</param>
    /// <returns>The user-scoped cache namespace.</returns>
    public static string ToUserCacheScope(this Guid currentUserPublicId)
    {
        // Keep the user-scope format centralized while preserving a receiver-shaped call site.
        return string.Format(USER_CACHE_SCOPE_FORMAT, currentUserPublicId);
    }
}
