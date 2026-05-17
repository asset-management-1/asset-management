namespace Authentication.Application.Policies;

/// <summary>
/// Resolves cache invalidation targets for current tenant profile vehicle deletion.
/// </summary>
public class DeleteUserVehicleCommandCachePolicy : ICacheInvalidationPolicy<DeleteUserVehicleCommand>
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Creates the vehicle-delete cache policy with current authenticated user scope access.
    /// </summary>
    /// <param name="authService">Provides access to the current authenticated principal.</param>
    public DeleteUserVehicleCommandCachePolicy(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Returns the current-user cache targets for the user whose vehicle list changed.
    /// </summary>
    /// <param name="request">The successfully handled vehicle deletion command.</param>
    /// <returns>The scoped cache target that should be invalidated.</returns>
    public IReadOnlyCollection<CacheInvalidationTargetModel> GetTargets(DeleteUserVehicleCommand request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var currentUserPublicId = _authService.UserId();
        if (!currentUserPublicId.HasValue)
        {
            return [];
        }

        var cacheScope = currentUserPublicId.Value.ToUserCacheScope();

        return
        [
            new CacheInvalidationTargetModel(USER_INFO_CACHE_KEY, cacheScope, failOnError: true),
            new CacheInvalidationTargetModel(USER_VEHICLES_CACHE_KEY, cacheScope, failOnError: true)
        ];
    }
}
