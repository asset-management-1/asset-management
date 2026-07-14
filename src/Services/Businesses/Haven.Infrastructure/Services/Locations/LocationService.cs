namespace Haven.Infrastructure.Services.Locations;

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
    /// Gets the hierarchical location context for supplied location codes.
    /// </summary>
    /// <param name="key">The requested location codes.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The resolved location context.</returns>
    public async Task<PropertyLocationContextModel> GetLocationsAsync(
        LocationKeyModel key,
        CancellationToken cancellationToken = default)
    {
        // Validate the caller-shaped key before reading hierarchical location data.
        ArgumentNullException.ThrowIfNull(key);

        // Resolve province first so lower levels can be validated under their selected parent.
        var province = await GetOptionalByCodeAsync(
            LOCATION_KIND_PROVINCE,
            key.ProvinceCode,
            _locationRepository.GetProvinceByCodeAsync,
            cancellationToken);

        // District is optional, but a supplied district code must belong to the supplied province.
        var district = await GetOptionalByCodeAsync(
            LOCATION_KIND_DISTRICT,
            key.DistrictCode,
            (code, token) => _locationRepository.GetDistrictByCodeAsync(code, province?.Id, token),
            cancellationToken);

        // Ward is optional, but a supplied ward code must belong to the supplied district.
        var ward = await GetOptionalByCodeAsync(
            LOCATION_KIND_WARD,
            key.WardCode,
            (code, token) => _locationRepository.GetWardByCodeAsync(code, district?.Id, token),
            cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.LocationLogs.LOCATION_CONTEXT_LOADED,
            key.ProvinceCode,
            key.DistrictCode,
            key.WardCode);

        // Return DB lookup rows as-is; callers decide which location parts are persisted.
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
        // Blank location codes are allowed because property address levels are optional in the current create/edit flow.
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        // Repository applies the parent scope when district/ward lookups depend on a resolved parent.
        var value = await getByCode(code, cancellationToken);

        if (value is not null)
        {
            return value;
        }

        // A supplied but unresolved code is an input problem, so return a consistent bad-request API error.
        _logger.LogWarning(
            InfrastructureLogConstants.LocationLogs.LOCATION_CODE_MISSING,
            kind,
            code);

        throw new ApiException(
            string.Format(CultureInfo.CurrentCulture, ApplicationErrorConstants.LookupErrors.ERROR_LOCATION_NOT_FOUND, kind, code),
            BAD_REQUEST,
            StatusCodes.Status400BadRequest);
    }
}
