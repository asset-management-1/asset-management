namespace Haven.Application.Interfaces.Services;

/// <summary>
/// Resolves province, district, and ward location values for Haven business workflows.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Gets a province by code.
    /// </summary>
    /// <param name="code">The province code.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The province lookup model, or <c>null</c>.</returns>
    Task<LocationLookupModel> GetProvinceByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a district by code and optional province parent.
    /// </summary>
    /// <param name="code">The district code.</param>
    /// <param name="provinceId">The optional parent province identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The district lookup model, or <c>null</c>.</returns>
    Task<LocationLookupModel> GetDistrictByCodeAsync(string code, long? provinceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a ward by code and optional district parent.
    /// </summary>
    /// <param name="code">The ward code.</param>
    /// <param name="districtId">The optional parent district identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The ward lookup model, or <c>null</c>.</returns>
    Task<LocationLookupModel> GetWardByCodeAsync(string code, long? districtId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the hierarchical location context for supplied location codes.
    /// </summary>
    /// <param name="key">The requested location codes.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The resolved location context.</returns>
    Task<PropertyLocationContextModel> GetLocationsAsync(LocationKeyModel key, CancellationToken cancellationToken = default);
}
