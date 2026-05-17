namespace Authentication.Application.Policies;

/// <summary>
/// Resolves cache invalidation targets for verified change-email updates.
/// </summary>
public class VerifyChangeEmailOtpCommandCachePolicy : ICacheInvalidationPolicy<VerifyChangeEmailOtpCommand>
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Creates the change-email verification cache policy with current authenticated user scope access.
    /// </summary>
    /// <param name="authService">Provides access to the current authenticated principal.</param>
    public VerifyChangeEmailOtpCommandCachePolicy(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Returns the current-user info cache target for the user whose email changed.
    /// </summary>
    /// <param name="request">The successfully handled change-email verification command.</param>
    /// <returns>The scoped cache target that should be invalidated.</returns>
    public IReadOnlyCollection<CacheInvalidationTargetModel> GetTargets(VerifyChangeEmailOtpCommand request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var currentUserPublicId = _authService.UserId();
        if (!currentUserPublicId.HasValue)
        {
            return [];
        }

        return [new CacheInvalidationTargetModel(USER_INFO_CACHE_KEY, currentUserPublicId.Value.ToUserCacheScope(), failOnError: true)];
    }
}
