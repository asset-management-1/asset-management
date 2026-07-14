namespace Authentication.Application.Policies;

/// <summary>
/// Resolves cache invalidation targets for the current-user profile update flow.
/// </summary>
public class UpdateUserInfoCommandCachePolicy : ICacheInvalidationPolicy<UpdateUserInfoCommand>
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Creates the profile-update cache policy with current authenticated user scope access.
    /// </summary>
    /// <param name="authService">Provides access to the current authenticated principal.</param>
    public UpdateUserInfoCommandCachePolicy(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Returns the current-user info cache target for the user whose profile changed.
    /// </summary>
    /// <param name="request">The successfully handled profile update command.</param>
    /// <returns>The scoped cache target that should be invalidated.</returns>
    public IReadOnlyCollection<CacheInvalidationTargetModel> GetTargets(UpdateUserInfoCommand request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Resolve the caller after profile mutation because the updated fields belong to that user's cache scope.
        var currentUserPublicId = _authService.UserId();

        if (!currentUserPublicId.HasValue)
        {
            // Background executions have no user-scoped profile cache entry to invalidate.
            return [];
        }

        // Invalidate the profile read model so the next read contains the submitted user-info changes.
        return [new CacheInvalidationTargetModel(USER_INFO_CACHE_KEY, currentUserPublicId.Value.ToUserCacheScope(), failOnError: true)];
    }
}
