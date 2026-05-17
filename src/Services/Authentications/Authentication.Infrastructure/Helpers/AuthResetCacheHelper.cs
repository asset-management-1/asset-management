namespace Authentication.Infrastructure.Helpers;

/// <summary>
/// Provides Authentication-module helpers for writing auth reset markers to cache.
/// </summary>
internal static class AuthResetCacheHelper
{
    /// <summary>
    /// Writes one user's auth reset marker to cache.
    /// </summary>
    /// <param name="cachingService">The cache service used to store the auth reset marker.</param>
    /// <param name="request">The auth reset cache marker write request.</param>
    /// <param name="cancellationToken">The token used to cancel the cache operation.</param>
    /// <returns>A task that completes when the cache write finishes.</returns>
    public static Task SetAsync(
        ICachingService cachingService,
        AuthResetCacheWriteModel request,
        CancellationToken cancellationToken)
    {
        // Store Unix milliseconds so the handler can compare marker and token issue time without date parsing.
        return cachingService.SetAbsoluteAsync(
            TokenHelper.BuildAuthResetCacheKey(request.UserPublicId),
            TokenHelper.ToAuthResetUnixMilliseconds(request.AuthResetAt),
            TokenHelper.GetAuthResetCacheTtl(request.RefreshTokenDays),
            cancellationToken);
    }
}
