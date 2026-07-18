namespace Haven.Infrastructure.Repositories.Properties;

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
        // The database unique constraint includes soft-deleted properties, so generation checks the full table.
        return _havenDbContext.Properties
            .AsNoTracking()
            .AnyAsync(x => x.PropertyCode == propertyCode, cancellationToken);
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
    /// Gets a tracked property graph for update/delete persistence.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tracked property graph, or <c>null</c>.</returns>
    public Task<Property> GetPropertyGraphByPublicIdAsync(
        Guid propertyPublicId,
        CancellationToken cancellationToken = default)
    {
        // EF tracking is required because update/delete synchronizes an aggregate graph in one transaction.
        return _havenDbContext.Properties
            .Include(property => property.Units.Where(unit => !unit.IsDeleted))
                .ThenInclude(unit => unit.UnitPackages.Where(unitPackage => !unitPackage.IsDeleted))
                .ThenInclude(unitPackage => unitPackage.Items.Where(item => !item.IsDeleted))
            .Include(property => property.UnitPackages.Where(unitPackage => !unitPackage.IsDeleted && unitPackage.UnitId == null))
                .ThenInclude(unitPackage => unitPackage.Items.Where(item => !item.IsDeleted))
            .Include(property => property.PropertyParties.Where(propertyParty => !propertyParty.IsDeleted))
            .Include(property => property.RentalChargePolicies.Where(policy => !policy.IsDeleted && policy.UnitId == null))
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                property => property.PublicId == propertyPublicId && !property.IsDeleted,
                cancellationToken);
    }

    /// <summary>
    /// Gets protected dependency counters for one property.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The mutation guard counters.</returns>
    public async Task<PropertyMutationGuardModel> GetPropertyMutationGuardAsync(
        Guid propertyPublicId,
        CancellationToken cancellationToken = default)
    {
        // Guard queries stay in Dapper because leasing, billing, vehicle, and document tables are read-only here.
        return await _dapperService.QueryFirstOrDefaultAsync<PropertyMutationGuardModel>(
            InfrastructureQueryConstants.GET_PROPERTY_MUTATION_GUARD_QUERY,
            new { PropertyPublicId = propertyPublicId },
            DapperCommandOptionsHelper.CreateText(cancellationToken)) ?? new PropertyMutationGuardModel();
    }

    /// <summary>
    /// Gets protected dependency counters for selected rooms.
    /// </summary>
    /// <param name="unitPublicIds">The room public identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room mutation guard counters.</returns>
    public async Task<IReadOnlyList<UnitMutationGuardModel>> GetUnitMutationGuardsAsync(
        IReadOnlyCollection<Guid> unitPublicIds,
        CancellationToken cancellationToken = default)
    {
        if (unitPublicIds.Count == 0)
        {
            return [];
        }

        // Unit guards are returned as rows so the service can evaluate only the rooms being changed.
        var rows = await _dapperService.QueryAsync<UnitMutationGuardModel>(
            InfrastructureQueryConstants.GET_UNIT_MUTATION_GUARDS_QUERY,
            new { UnitPublicIds = unitPublicIds.ToArray() },
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Gets invoice-line dependency counters for selected charge policies.
    /// </summary>
    /// <param name="policyPublicIds">The charge policy public identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The charge policy mutation guard counters.</returns>
    public async Task<IReadOnlyList<ChargePolicyMutationGuardModel>> GetChargePolicyMutationGuardsAsync(
        IReadOnlyCollection<Guid> policyPublicIds,
        CancellationToken cancellationToken = default)
    {
        if (policyPublicIds.Count == 0)
        {
            return [];
        }

        // Charge policies can be removed only when no invoice line has already referenced them.
        var rows = await _dapperService.QueryAsync<ChargePolicyMutationGuardModel>(
            InfrastructureQueryConstants.GET_CHARGE_POLICY_MUTATION_GUARDS_QUERY,
            new { PolicyPublicIds = policyPublicIds.ToArray() },
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Gets contract dependency counters for selected package templates.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="packagePublicIds">The package template public identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The package template mutation guard counters.</returns>
    public async Task<IReadOnlyList<PackageTemplateMutationGuardModel>> GetPackageTemplateMutationGuardsAsync(
        Guid propertyPublicId,
        IReadOnlyCollection<Guid> packagePublicIds,
        CancellationToken cancellationToken = default)
    {
        var ids = packagePublicIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();
        if (ids.Length == 0)
        {
            return [];
        }

        // Common package templates are property-level rows, so dependency checks are grouped by public id.
        var rows = await _dapperService.QueryAsync<PackageTemplateMutationGuardModel>(
            InfrastructureQueryConstants.GET_PACKAGE_TEMPLATE_MUTATION_GUARDS_QUERY,
            new { PropertyPublicId = propertyPublicId, PackagePublicIds = ids },
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
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
        // No parent property ids means there is no derived floor query to run.
        if (parameters.PropertyPublicIds.Count == 0)
        {
            return [];
        }

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
        // No parent property ids means there is no room-card query to run.
        if (parameters.PropertyPublicIds.Count == 0)
        {
            return [];
        }

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
        // No parent property ids means tenant rows cannot be scoped to room cards.
        if (parameters.PropertyPublicIds.Count == 0)
        {
            return [];
        }

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
        // Package template rows come from property-level common packages; room overrides are handled by room APIs.
        var rows = await _dapperService.QueryAsync<PropertyPackageTemplateRowModel>(
            InfrastructureQueryConstants.GET_PROPERTY_PACKAGE_TEMPLATES_QUERY,
            new { PropertyPublicId = propertyPublicId },
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Gets the active whole-building rental contract for one property.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The whole-building rental contract row, or <c>null</c>.</returns>
    public Task<PropertyWholeBuildingRentalRowModel> GetWholeBuildingRentalAsync(
        Guid propertyPublicId,
        CancellationToken cancellationToken = default)
    {
        // Whole-building rental is represented by a contract on the representative building unit.
        return _dapperService.QueryFirstOrDefaultAsync<PropertyWholeBuildingRentalRowModel>(
            InfrastructureQueryConstants.GET_PROPERTY_WHOLE_BUILDING_RENTAL_QUERY,
            new { PropertyPublicId = propertyPublicId },
            DapperCommandOptionsHelper.CreateText(cancellationToken));
    }

}
