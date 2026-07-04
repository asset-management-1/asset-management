namespace Haven.Application.Interfaces.Repositories;

/// <summary>
/// Provides repository operations for properties.
/// </summary>
public interface IPropertyRepository : IGenericRepository<Property>
{
    /// <summary>
    /// Checks whether a property code already exists.
    /// </summary>
    /// <param name="propertyCode">The property code to check.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns><c>true</c> when the code already exists.</returns>
    Task<bool> PropertyCodeExistsAsync(string propertyCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one page of property header rows with total count metadata.
    /// </summary>
    /// <param name="parameters">The list query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The property page rows.</returns>
    Task<IReadOnlyList<PropertyRowModel>> GetPropertyPageAsync(
        PropertyListQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one property detail header scoped by current party.
    /// </summary>
    /// <param name="parameters">The scoped property query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The property detail row, or <c>null</c>.</returns>
    Task<PropertyRowModel> GetPropertyDetailHeaderAsync(
        PropertyScopedQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets floor summary rows for selected properties.
    /// </summary>
    /// <param name="parameters">The child row query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The floor summary rows.</returns>
    Task<IReadOnlyList<PropertyFloorRowModel>> GetFloorsAsync(
        PropertyChildRowsQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets room rows for selected properties.
    /// </summary>
    /// <param name="parameters">The child row query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room rows.</returns>
    Task<IReadOnlyList<PropertyRoomRowModel>> GetRoomsAsync(
        PropertyChildRowsQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active tenant rows for room cards.
    /// </summary>
    /// <param name="parameters">The child row query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room tenant rows.</returns>
    Task<IReadOnlyList<PropertyRoomTenantRowModel>> GetRoomTenantsAsync(
        PropertyChildRowsQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets charge-policy rows for one property.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The charge-policy rows.</returns>
    Task<IReadOnlyList<PropertyChargePolicyRowModel>> GetChargePoliciesAsync(
        Guid propertyPublicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets package template rows for one property.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The package template rows.</returns>
    Task<IReadOnlyList<PropertyPackageTemplateRowModel>> GetPackageTemplatesAsync(
        Guid propertyPublicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets management summary counters for one property.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The management summary row.</returns>
    Task<PropertyManagementSummaryRowModel> GetManagementSummaryAsync(
        Guid propertyPublicId,
        CancellationToken cancellationToken = default);
}
