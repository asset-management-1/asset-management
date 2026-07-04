namespace Haven.Application.Interfaces.Repositories;

/// <summary>
/// Provides province, district, and ward read operations.
/// </summary>
public interface ILocationRepository
{
    /// <summary>
    /// Gets a province by code.
    /// </summary>
    /// <param name="code">The province code.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The province lookup model, or <c>null</c>.</returns>
    Task<LocationLookupModel> GetProvinceByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a district by code and optional province parent.
    /// </summary>
    /// <param name="code">The district code.</param>
    /// <param name="provinceId">The optional parent province identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The district lookup model, or <c>null</c>.</returns>
    Task<LocationLookupModel> GetDistrictByCodeAsync(string code, long? provinceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a ward by code and optional district parent.
    /// </summary>
    /// <param name="code">The ward code.</param>
    /// <param name="districtId">The optional parent district identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The ward lookup model, or <c>null</c>.</returns>
    Task<LocationLookupModel> GetWardByCodeAsync(string code, long? districtId, CancellationToken cancellationToken = default);
}
