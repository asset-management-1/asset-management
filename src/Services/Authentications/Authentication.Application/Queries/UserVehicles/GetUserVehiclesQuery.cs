namespace Authentication.Application.Queries.UserVehicles;

/// <summary>
/// Represents a request to load vehicles for the current tenant profile.
/// </summary>
public class GetUserVehiclesQuery : IQuery<ResponseDto<IReadOnlyList<UserVehicleResponseDto>>>, ICacheableMediatorQueryService
{
    /// <summary>
    /// Gets a value indicating whether this query should bypass the cache layer.
    /// </summary>
    public bool BypassCache { get; init; }

    /// <summary>
    /// Gets the logical cache group key for the current tenant profile vehicles query.
    /// </summary>
    public string CacheKey => USER_VEHICLES_CACHE_KEY;

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
