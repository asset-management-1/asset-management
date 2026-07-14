namespace Authentication.Application.Policies;

/// <summary>
/// Resolves cache invalidation targets for the party-context switch flow.
/// </summary>
public class SwitchPartyCommandCachePolicy : ICacheInvalidationPolicy<SwitchPartyCommand>
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Creates the switch-party cache policy with current authenticated user scope access.
    /// </summary>
    /// <param name="authService">Provides access to the current authenticated principal.</param>
    public SwitchPartyCommandCachePolicy(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Returns the current-user cache targets for the user whose active context changed.
    /// </summary>
    /// <param name="request">The successfully handled switch-party command.</param>
    /// <returns>The scoped cache target that should be invalidated.</returns>
    public IReadOnlyCollection<CacheInvalidationTargetModel> GetTargets(SwitchPartyCommand request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Resolve the caller after switching context because the active party belongs to that user's profile projection.
        var currentUserPublicId = _authService.UserId();

        if (!currentUserPublicId.HasValue)
        {
            // Background executions have no user-scoped profile cache entry to invalidate.
            return [];
        }

        // Build the same per-user scope used by the profile read cache before publishing the invalidation target.
        var cacheScope = currentUserPublicId.Value.ToUserCacheScope();

        // Invalidate the profile read model so the next read reflects the selected active party.
        return [new CacheInvalidationTargetModel(USER_INFO_CACHE_KEY, cacheScope, failOnError: true)];
    }
}
