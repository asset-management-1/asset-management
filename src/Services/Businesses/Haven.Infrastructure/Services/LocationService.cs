namespace Haven.Infrastructure.Services;

/// <summary>
/// Resolves location values and validates province/district/ward hierarchy.
/// </summary>
public class LocationService : ILocationService
{
    private readonly ILocationRepository _locationRepository;
    private readonly ILogger<LocationService> _logger;

    /// <summary>
    /// Creates the location service.
    /// </summary>
    /// <param name="locationRepository">The location repository.</param>
    /// <param name="logger">The service logger.</param>
    public LocationService(
        ILocationRepository locationRepository,
        ILogger<LocationService> logger)
    {
        _locationRepository = locationRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets a province by code.
    /// </summary>
    /// <param name="code">The province code.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The province lookup model, or <c>null</c>.</returns>
    public Task<LocationLookupModel> GetProvinceByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return _locationRepository.GetProvinceByCodeAsync(code, cancellationToken);
    }

    /// <summary>
    /// Gets a district by code and optional province parent.
    /// </summary>
    /// <param name="code">The district code.</param>
    /// <param name="provinceId">The optional parent province identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The district lookup model, or <c>null</c>.</returns>
    public Task<LocationLookupModel> GetDistrictByCodeAsync(
        string code,
        long? provinceId,
        CancellationToken cancellationToken = default)
    {
        return _locationRepository.GetDistrictByCodeAsync(code, provinceId, cancellationToken);
    }

    /// <summary>
    /// Gets a ward by code and optional district parent.
    /// </summary>
    /// <param name="code">The ward code.</param>
    /// <param name="districtId">The optional parent district identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The ward lookup model, or <c>null</c>.</returns>
    public Task<LocationLookupModel> GetWardByCodeAsync(
        string code,
        long? districtId,
        CancellationToken cancellationToken = default)
    {
        return _locationRepository.GetWardByCodeAsync(code, districtId, cancellationToken);
    }

    /// <summary>
    /// Gets the hierarchical location context for supplied location codes.
    /// </summary>
    /// <param name="key">The requested location codes.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The resolved location context.</returns>
    public async Task<PropertyLocationContextModel> GetLocationsAsync(
        LocationKeyModel key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        var province = await GetOptionalByCodeAsync(
            LOCATION_KIND_PROVINCE,
            key.ProvinceCode,
            GetProvinceByCodeAsync,
            cancellationToken);
        var district = await GetOptionalByCodeAsync(
            LOCATION_KIND_DISTRICT,
            key.DistrictCode,
            (code, token) => GetDistrictByCodeAsync(code, province?.Id, token),
            cancellationToken);
        var ward = await GetOptionalByCodeAsync(
            LOCATION_KIND_WARD,
            key.WardCode,
            (code, token) => GetWardByCodeAsync(code, district?.Id, token),
            cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.LocationLogs.LOCATION_CONTEXT_LOADED,
            province is not null,
            district is not null,
            ward is not null);

        return new PropertyLocationContextModel
        {
            Province = province,
            District = district,
            Ward = ward
        };
    }

    /// <summary>
    /// Gets an optional lookup value by code and throws a bad-request error when invalid.
    /// </summary>
    /// <typeparam name="T">The lookup model type.</typeparam>
    /// <param name="kind">The location kind used in the error message.</param>
    /// <param name="code">The optional location code.</param>
    /// <param name="getByCode">The lookup function.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The lookup value, or <c>null</c> when no code is supplied.</returns>
    private async Task<T> GetOptionalByCodeAsync<T>(
        string kind,
        string code,
        Func<string, CancellationToken, Task<T>> getByCode,
        CancellationToken cancellationToken)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var value = await getByCode(code, cancellationToken);
        if (value is not null)
        {
            return value;
        }

        _logger.LogWarning(
            InfrastructureLogConstants.LocationLogs.LOCATION_CODE_MISSING,
            kind,
            code);

        throw new ApiException(
            string.Format(CultureInfo.CurrentCulture, ERROR_LOCATION_NOT_FOUND, kind, code),
            BAD_REQUEST,
            StatusCodes.Status400BadRequest);
    }
}
