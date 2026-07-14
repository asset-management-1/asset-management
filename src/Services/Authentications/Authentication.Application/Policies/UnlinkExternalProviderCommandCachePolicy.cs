namespace Authentication.Application.Policies;

/// <summary>
/// Resolves cache invalidation targets for the external-provider unlink flow.
/// </summary>
public class UnlinkExternalProviderCommandCachePolicy : ICacheInvalidationPolicy<UnlinkExternalProviderCommand>
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Creates the external-provider unlink cache policy with current authenticated user scope access.
    /// </summary>
    /// <param name="authService">Provides access to the current authenticated principal.</param>
    public UnlinkExternalProviderCommandCachePolicy(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Returns the current-user info cache target for the user whose provider list changed.
    /// </summary>
    /// <param name="request">The successfully handled external-provider unlink command.</param>
    /// <returns>The scoped cache target that should be invalidated.</returns>
    public IReadOnlyCollection<CacheInvalidationTargetModel> GetTargets(UnlinkExternalProviderCommand request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Resolve the caller after unlinking succeeds because provider changes affect only that user's profile projection.
        var currentUserPublicId = _authService.UserId();

        if (!currentUserPublicId.HasValue)
        {
            // Background executions have no user-scoped profile cache entry to invalidate.
            return [];
        }

        // Invalidate the profile read model so the next read contains the updated provider list.
        return
        [
            new CacheInvalidationTargetModel(USER_INFO_CACHE_KEY, currentUserPublicId.Value.ToUserCacheScope(), failOnError: true)
        ];
    }
}
