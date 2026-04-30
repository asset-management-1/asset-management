namespace Be.Haven.Cache.Interfaces;

/// <summary>
/// Provides caching utilities for handling OData pagination (nextLink, previousLink, top values) per user.
/// </summary>
public interface IODataPaginationCacheService
{
    /// <summary>
    /// Retrieves the nextLink value from cache using the supplied link identifier.
    /// </summary>
    public Task<string> GetNextLinkAsync(string nextLinkId);

    /// <summary>
    /// Stores a nextLink value in cache for the specified action and page index, and returns the generated cache key.
    /// </summary>
    public Task<string> StoreNextLinkAsync(string actionName, int pageIndex, string nextLinkValue);

    /// <summary>
    /// Retrieves the previous link key associated with the given current link key.
    /// </summary>
    public Task<string> GetPreviousLinkIdAsync(string currentLinkId, string actionName);

    /// <summary>
    /// Removes all cached pagination-related keys belonging to the current user.
    /// </summary>
    public Task RemoveAllCacheValuesOfUserAsync(string actionName);

    /// <summary>
    /// Validates whether the provided link identifier points to an existing cached nextLink value.
    /// </summary>
    public Task<bool> ValidateLinkIdAsync(string linkId);

    /// <summary>
    /// Retrieves the current authenticated user's identifier for cache scoping.
    /// </summary>
    public string GetJwtId();
}

