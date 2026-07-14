namespace Haven.Infrastructure.Repositories.Locations;

/// <summary>
/// Provides location persistence reads.
/// </summary>
public class LocationRepository : ILocationRepository
{
    private readonly IDapperService _dapperService;

    /// <summary>
    /// Creates the location repository.
    /// </summary>
    /// <param name="dapperService">The shared Dapper service.</param>
    public LocationRepository(IDapperService dapperService)
    {
        _dapperService = dapperService;
    }

    /// <summary>
    /// Gets a province by code.
    /// </summary>
    /// <param name="code">The province code.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The province lookup model, or <c>null</c>.</returns>
    public Task<LocationLookupModel> GetProvinceByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        // Province lookup has no parent constraint because it is the root location level.
        return QueryLocationAsync(
            InfrastructureQueryConstants.GET_PROVINCE_BY_CODE_QUERY,
            new { Code = code },
            cancellationToken);
    }

    /// <summary>
    /// Gets a district by code and optional province parent.
    /// </summary>
    /// <param name="code">The district code.</param>
    /// <param name="provinceId">The optional parent province identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The district lookup model, or <c>null</c>.</returns>
    public Task<LocationLookupModel> GetDistrictByCodeAsync(
        string code,
        long? provinceId,
        CancellationToken cancellationToken = default)
    {
        // When supplied, the province id prevents a valid district code from crossing the selected hierarchy.
        return QueryLocationAsync(
            InfrastructureQueryConstants.GET_DISTRICT_BY_CODE_QUERY,
            new { Code = code, ProvinceId = provinceId },
            cancellationToken);
    }

    /// <summary>
    /// Gets a ward by code and optional district parent.
    /// </summary>
    /// <param name="code">The ward code.</param>
    /// <param name="districtId">The optional parent district identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The ward lookup model, or <c>null</c>.</returns>
    public Task<LocationLookupModel> GetWardByCodeAsync(
        string code,
        long? districtId,
        CancellationToken cancellationToken = default)
    {
        // When supplied, the district id keeps the ward lookup inside the submitted address hierarchy.
        return QueryLocationAsync(
            InfrastructureQueryConstants.GET_WARD_BY_CODE_QUERY,
            new { Code = code, DistrictId = districtId },
            cancellationToken);
    }

    /// <summary>
    /// Runs a location lookup query with the supplied parameters.
    /// </summary>
    /// <param name="sql">The SQL query.</param>
    /// <param name="parameters">The query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The location lookup model, or <c>null</c>.</returns>
    private Task<LocationLookupModel> QueryLocationAsync(
        string sql,
        object parameters,
        CancellationToken cancellationToken)
    {
        // A missing parameter object represents no lookup request and avoids sending an unbounded query.
        if (parameters is null)
        {
            return Task.FromResult<LocationLookupModel>(null);
        }

        // Location reads are projection-only, so Dapper returns the compact lookup model directly.
        return _dapperService.QueryFirstOrDefaultAsync<LocationLookupModel>(
            sql,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));
    }
}
