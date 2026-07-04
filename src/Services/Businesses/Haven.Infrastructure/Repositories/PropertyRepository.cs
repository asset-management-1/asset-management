namespace Haven.Infrastructure.Repositories;

/// <summary>
/// Provides persistence and screen-query operations for properties.
/// </summary>
public class PropertyRepository : GenericRepository<Property>, IPropertyRepository
{
    private readonly HavenDbContext _havenDbContext;
    private readonly IDapperService _dapperService;

    /// <summary>
    /// Creates the property repository.
    /// </summary>
    /// <param name="dbContext">The Haven business database context.</param>
    /// <param name="dapperService">The shared Dapper service.</param>
    public PropertyRepository(
        HavenDbContext dbContext,
        IDapperService dapperService) : base(dbContext)
    {
        _havenDbContext = dbContext;
        _dapperService = dapperService;
    }

    /// <summary>
    /// Checks whether a property code already exists.
    /// </summary>
    /// <param name="propertyCode">The property code to check.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns><c>true</c> when the property code already exists.</returns>
    public Task<bool> PropertyCodeExistsAsync(string propertyCode, CancellationToken cancellationToken = default)
    {
        // Code uniqueness is global in asset.Properties, so a simple existence check is enough.
        return _havenDbContext.Properties
            .AsNoTracking()
            .AnyAsync(x => x.PropertyCode == propertyCode && !x.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets one page of property header rows with total count metadata.
    /// </summary>
    /// <param name="parameters">The list query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The property page rows.</returns>
    public async Task<IReadOnlyList<PropertyRowModel>> GetPropertyPageAsync(
        PropertyListQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // The list page is loaded as flat rows; nesting is assembled by the property mapper.
        var rows = await _dapperService.QueryAsync<PropertyRowModel>(
            InfrastructureQueryConstants.GET_PROPERTY_LIST_PAGE_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Gets one property detail header scoped by current party.
    /// </summary>
    /// <param name="parameters">The scoped property query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The property detail row, or <c>null</c>.</returns>
    public Task<PropertyRowModel> GetPropertyDetailHeaderAsync(
        PropertyScopedQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // Detail header lookup includes party scope so inaccessible properties resolve as not found.
        return _dapperService.QueryFirstOrDefaultAsync<PropertyRowModel>(
            InfrastructureQueryConstants.GET_PROPERTY_DETAIL_HEADER_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));
    }

    /// <summary>
    /// Gets floor summary rows for selected properties.
    /// </summary>
    /// <param name="parameters">The child row query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The floor summary rows.</returns>
    public async Task<IReadOnlyList<PropertyFloorRowModel>> GetFloorsAsync(
        PropertyChildRowsQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // Floors are summaries derived from unit rows, not records from a separate floor table.
        var rows = await _dapperService.QueryAsync<PropertyFloorRowModel>(
            InfrastructureQueryConstants.GET_PROPERTY_FLOORS_BY_PUBLIC_IDS_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Gets room rows for selected properties.
    /// </summary>
    /// <param name="parameters">The child row query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room rows.</returns>
    public async Task<IReadOnlyList<PropertyRoomRowModel>> GetRoomsAsync(
        PropertyChildRowsQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // Room cards need joined master-data, tenant, contract, and invoice status fields.
        var rows = await _dapperService.QueryAsync<PropertyRoomRowModel>(
            InfrastructureQueryConstants.GET_PROPERTY_ROOMS_BY_PUBLIC_IDS_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Gets active tenant rows for room cards.
    /// </summary>
    /// <param name="parameters">The child row query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room tenant rows.</returns>
    public async Task<IReadOnlyList<PropertyRoomTenantRowModel>> GetRoomTenantsAsync(
        PropertyChildRowsQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // Room cards use tenants as the single source for tenant names and contract dates.
        var rows = await _dapperService.QueryAsync<PropertyRoomTenantRowModel>(
            InfrastructureQueryConstants.GET_PROPERTY_ROOM_TENANTS_BY_PUBLIC_IDS_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Gets charge-policy rows for one property.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The charge-policy rows.</returns>
    public async Task<IReadOnlyList<PropertyChargePolicyRowModel>> GetChargePoliciesAsync(
        Guid propertyPublicId,
        CancellationToken cancellationToken = default)
    {
        // Charge policies are property-level setup data shown on the building detail screen.
        var rows = await _dapperService.QueryAsync<PropertyChargePolicyRowModel>(
            InfrastructureQueryConstants.GET_PROPERTY_CHARGE_POLICIES_QUERY,
            new { PropertyPublicId = propertyPublicId },
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Gets package template rows for one property.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The package template rows.</returns>
    public async Task<IReadOnlyList<PropertyPackageTemplateRowModel>> GetPackageTemplatesAsync(
        Guid propertyPublicId,
        CancellationToken cancellationToken = default)
    {
        // Unit package rows are cloned per room; the response mapper collapses them into distinct templates.
        var rows = await _dapperService.QueryAsync<PropertyPackageTemplateRowModel>(
            InfrastructureQueryConstants.GET_PROPERTY_PACKAGE_TEMPLATES_QUERY,
            new { PropertyPublicId = propertyPublicId },
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Gets management summary counters for one property.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The management summary row.</returns>
    public async Task<PropertyManagementSummaryRowModel> GetManagementSummaryAsync(
        Guid propertyPublicId,
        CancellationToken cancellationToken = default)
    {
        // Missing optional child data should surface as zero counters, not a null response section.
        return await _dapperService.QueryFirstOrDefaultAsync<PropertyManagementSummaryRowModel>(
            InfrastructureQueryConstants.GET_PROPERTY_MANAGEMENT_SUMMARY_QUERY,
            new { PropertyPublicId = propertyPublicId },
            DapperCommandOptionsHelper.CreateText(cancellationToken)) ?? new PropertyManagementSummaryRowModel();
    }
}
