namespace Authentication.Application.Queries.UserInfo;

/// <summary>
/// Represents a request to load current authenticated user information.
/// </summary>
public class GetUserInfoQuery : IQuery<ResponseDto<UserInfoResponseDto>>, ICacheableMediatorQueryService
{
    // Cache bypass is an internal pipeline concern and is never exposed as a public query-string parameter.
    bool ICacheableMediatorQueryService.BypassCache => false;

    /// <summary>
    /// Gets the logical cache group key for the current-user profile query.
    /// </summary>
    public string CacheKey => USER_INFO_CACHE_KEY;

    /// <summary>
    /// Gets the current-user scope sentinel for this query.
    /// The caching behavior resolves it into the concrete authenticated user scope at runtime.
    /// </summary>
    public string CacheScope => CURRENT_USER_CACHE_SCOPE;

    /// <summary>
    /// Gets the optional cache lifetime requested by the caller.
    /// When not provided, cache data uses <c>CacheSettings:AbsoluteExpiration</c>.
    /// </summary>
    public TimeSpan? AbsoluteExpiration { get; init; }
}
