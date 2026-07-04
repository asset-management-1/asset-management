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

        var currentUserPublicId = _authService.UserId();
        if (!currentUserPublicId.HasValue)
        {
            return [];
        }

        var cacheScope = currentUserPublicId.Value.ToUserCacheScope();

        return [new CacheInvalidationTargetModel(USER_INFO_CACHE_KEY, cacheScope, failOnError: true)];
    }
}
