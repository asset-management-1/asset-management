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
    /// Gets a tracked property graph for update/delete persistence.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tracked property graph, or <c>null</c>.</returns>
    Task<Property> GetPropertyGraphByPublicIdAsync(
        Guid propertyPublicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets protected dependency counters for one property.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The mutation guard counters.</returns>
    Task<PropertyMutationGuardModel> GetPropertyMutationGuardAsync(
        Guid propertyPublicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets protected dependency counters for selected rooms.
    /// </summary>
    /// <param name="unitPublicIds">The room public identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room mutation guard counters.</returns>
    Task<IReadOnlyList<UnitMutationGuardModel>> GetUnitMutationGuardsAsync(
        IReadOnlyCollection<Guid> unitPublicIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets invoice-line dependency counters for selected charge policies.
    /// </summary>
    /// <param name="policyPublicIds">The charge policy public identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The charge policy mutation guard counters.</returns>
    Task<IReadOnlyList<ChargePolicyMutationGuardModel>> GetChargePolicyMutationGuardsAsync(
        IReadOnlyCollection<Guid> policyPublicIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets contract dependency counters for selected package templates.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="packagePublicIds">The package template public identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The package template mutation guard counters.</returns>
    Task<IReadOnlyList<PackageTemplateMutationGuardModel>> GetPackageTemplateMutationGuardsAsync(
        Guid propertyPublicId,
        IReadOnlyCollection<Guid> packagePublicIds,
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
    /// Gets the active whole-building rental contract for one property.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The whole-building rental contract row, or <c>null</c>.</returns>
    Task<PropertyWholeBuildingRentalRowModel> GetWholeBuildingRentalAsync(
        Guid propertyPublicId,
        CancellationToken cancellationToken = default);

}
