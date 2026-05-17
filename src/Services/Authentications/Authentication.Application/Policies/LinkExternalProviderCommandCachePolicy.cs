namespace Authentication.Application.Policies;

/// <summary>
/// Resolves cache invalidation targets for the external-provider link flow.
/// </summary>
public class LinkExternalProviderCommandCachePolicy : ICacheInvalidationPolicy<LinkExternalProviderCommand>
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Creates the external-provider link cache policy with current authenticated user scope access.
    /// </summary>
    /// <param name="authService">Provides access to the current authenticated principal.</param>
    public LinkExternalProviderCommandCachePolicy(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Returns the user-info cache target for the authenticated user whose provider list changed.
    /// </summary>
    /// <param name="request">The successfully handled external-provider link command.</param>
    /// <returns>The scoped cache target that should be invalidated.</returns>
    public IReadOnlyCollection<CacheInvalidationTargetModel> GetTargets(LinkExternalProviderCommand request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var currentUserPublicId = _authService.UserId();
        if (!currentUserPublicId.HasValue)
        {
            return [];
        }

        return
        [
            new CacheInvalidationTargetModel(USER_INFO_CACHE_KEY, currentUserPublicId.Value.ToUserCacheScope(), failOnError: true)
        ];
    }
}
