namespace Authentication.Application.Policies;

/// <summary>
/// Resolves cache invalidation targets for current-user KYC submissions.
/// </summary>
public class SubmitKycCommandCachePolicy : ICacheInvalidationPolicy<SubmitKycCommand>
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Creates the KYC cache policy with current authenticated user scope access.
    /// </summary>
    /// <param name="authService">Provides access to the current authenticated principal.</param>
    public SubmitKycCommandCachePolicy(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Returns the current-user info cache target for the user whose KYC state changed.
    /// </summary>
    /// <param name="request">The successfully handled KYC submission command.</param>
    /// <returns>The scoped cache target that should be invalidated.</returns>
    public IReadOnlyCollection<CacheInvalidationTargetModel> GetTargets(SubmitKycCommand request)
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
