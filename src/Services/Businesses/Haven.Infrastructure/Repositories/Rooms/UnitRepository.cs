namespace Haven.Infrastructure.Repositories.Rooms;

/// <summary>
/// Provides EF write operations for units.
/// </summary>
public class UnitRepository : GenericRepository<Unit>, IUnitRepository
{
    private readonly HavenDbContext _havenDbContext;
    private readonly IDapperService _dapperService;

    /// <summary>
    /// Creates the unit repository.
    /// </summary>
    /// <param name="dbContext">The Haven business database context.</param>
    /// <param name="dapperService">The shared Dapper service.</param>
    public UnitRepository(
        HavenDbContext dbContext,
        IDapperService dapperService) : base(dbContext)
    {
        _havenDbContext = dbContext;
        _dapperService = dapperService;
    }

    /// <summary>
    /// Loads a paged room ledger read model.
    /// </summary>
    /// <param name="parameters">The room list query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room list rows.</returns>
    public async Task<IReadOnlyList<RoomListRoomRowModel>> GetRoomPageAsync(
        RoomListQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // Room ledger is a screen read model with filters, grouping metadata, and aggregates.
        var rows = await _dapperService.QueryAsync<RoomListRoomRowModel>(
            InfrastructureQueryConstants.GET_ROOM_LIST_PAGE_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Loads active tenant rows for the supplied rooms.
    /// </summary>
    /// <param name="unitPublicIds">The room public identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room tenant rows.</returns>
    public async Task<IReadOnlyList<RoomTenantRowModel>> GetRoomTenantsAsync(
        IReadOnlyCollection<Guid> unitPublicIds,
        CancellationToken cancellationToken = default)
    {
        if (unitPublicIds.Count == 0)
        {
            return [];
        }

        // Tenant rows are loaded separately so list/detail responses can keep one stable tenant shape.
        var rows = await _dapperService.QueryAsync<RoomTenantRowModel>(
            InfrastructureQueryConstants.GET_ROOM_TENANTS_BY_PUBLIC_IDS_QUERY,
            new { UnitPublicIds = unitPublicIds.ToArray() },
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Loads one room detail row inside the current party scope.
    /// </summary>
    /// <param name="parameters">The scoped room parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room detail row, or <c>null</c>.</returns>
    public Task<RoomDetailRowModel> GetRoomDetailAsync(
        RoomScopedQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // Detail lookup includes party scope so inaccessible rooms resolve as not found.
        return _dapperService.QueryFirstOrDefaultAsync<RoomDetailRowModel>(
            InfrastructureQueryConstants.GET_ROOM_DETAIL_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));
    }

    /// <summary>
    /// Loads effective charge policies for a room.
    /// </summary>
    /// <param name="roomPublicId">The room public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The effective charge policy rows.</returns>
    public async Task<IReadOnlyList<RoomChargePolicyRowModel>> GetEffectiveChargePoliciesAsync(
        Guid roomPublicId,
        CancellationToken cancellationToken = default)
    {
        // Effective policy rows prefer room overrides and fallback to property-level common setup.
        var rows = await _dapperService.QueryAsync<RoomChargePolicyRowModel>(
            InfrastructureQueryConstants.GET_ROOM_EFFECTIVE_CHARGE_POLICIES_QUERY,
            new { RoomPublicId = roomPublicId },
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Loads effective package templates for a room.
    /// </summary>
    /// <param name="roomPublicId">The room public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The effective package template rows.</returns>
    public async Task<IReadOnlyList<RoomPackageTemplateRowModel>> GetEffectivePackageTemplatesAsync(
        RoomPackageQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // Effective package rows stay landlord-scoped and prefer room overrides over common property setup.
        var rows = await _dapperService.QueryAsync<RoomPackageTemplateRowModel>(
            InfrastructureQueryConstants.GET_ROOM_EFFECTIVE_PACKAGE_TEMPLATES_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Locks and loads the tracked package graph for one landlord-scoped room.
    /// </summary>
    /// <param name="parameters">The scoped room query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the lock and graph load.</param>
    /// <returns>The tracked room package graph, or <c>null</c>.</returns>
    public async Task<Unit> GetRoomPackageGraphForMutationAsync(
        RoomScopedQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // Lock only after the same active relationship-code scope used by room reads has been applied.
        var lockedRoomId = await _havenDbContext.Database
            .SqlQueryRaw<long>(
                InfrastructureQueryConstants.LOCK_ROOM_FOR_PACKAGE_MUTATION_QUERY,
                parameters.RoomPublicId,
                parameters.CurrentPartyId,
                parameters.RelationshipCodes.ToArray())
            .SingleOrDefaultAsync(cancellationToken);

        if (lockedRoomId == 0)
        {
            return null;
        }

        // Load active room overrides and common packages only after the lock is held, preventing stale materialization decisions.
        return await _havenDbContext.Units
            .Include(unit => unit.UnitPackages.Where(package => !package.IsDeleted))
                .ThenInclude(package => package.Items.Where(item => !item.IsDeleted))
            .Include(unit => unit.Property)
                .ThenInclude(property => property.UnitPackages.Where(package => package.UnitId == null && !package.IsDeleted))
                    .ThenInclude(package => package.Items.Where(item => !item.IsDeleted))
            .FirstOrDefaultAsync(unit => unit.Id == lockedRoomId, cancellationToken);
    }

    /// <summary>
    /// Checks whether a unit code is already used by any room in one property.
    /// </summary>
    /// <param name="propertyId">The internal property identifier.</param>
    /// <param name="unitCode">The backend-generated unit business code.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns><c>true</c> when the code already exists under the property.</returns>
    public Task<bool> ExistsUnitCodeAsync(
        long propertyId,
        string unitCode,
        CancellationToken cancellationToken = default)
    {
        // The database unique constraint includes soft-deleted units, so the lookup must use the full property scope.
        return _havenDbContext.Units
            .AsNoTracking()
            .AnyAsync(
                unit => unit.PropertyId == propertyId
                        && unit.UnitCode == unitCode,
                cancellationToken);
    }

    /// <summary>
    /// Loads a tracked room graph under the current-party property scope.
    /// </summary>
    /// <param name="parameters">The scoped room parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tracked room graph, or <c>null</c>.</returns>
    public Task<Unit> GetRoomGraphAsync(
        RoomScopedQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        var relationshipCodes = parameters.RelationshipCodes.ToArray();

        // EF applies authorization inside the scoped graph load; split includes keep navigation loading bounded.
        return _havenDbContext.Units
            .FromSqlRaw(
                InfrastructureQueryConstants.GET_SCOPED_ROOM_GRAPH_QUERY,
                parameters.RoomPublicId,
                parameters.CurrentPartyId,
                relationshipCodes)
            .Include(unit => unit.Property)
                .ThenInclude(property => property.Units.Where(propertyUnit => !propertyUnit.IsDeleted))
            .Include(unit => unit.UnitPackages.Where(unitPackage => !unitPackage.IsDeleted))
                .ThenInclude(unitPackage => unitPackage.Items.Where(item => !item.IsDeleted))
            .Include(unit => unit.RentalChargePolicies.Where(policy => !policy.IsDeleted))
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Loads the tracked room fields required by a landlord-scoped occupancy mutation.
    /// </summary>
    /// <param name="parameters">The scoped room parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tracked room and property, or <c>null</c>.</returns>
    public Task<Unit> GetRoomForOccupancyMutationAsync(
        RoomScopedQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        var relationshipCodes = parameters.RelationshipCodes.ToArray();

        // Occupancy mutations need room capacity, pricing, and property identity without package or policy graphs.
        return _havenDbContext.Units
            .FromSqlRaw(
                InfrastructureQueryConstants.GET_SCOPED_ROOM_GRAPH_QUERY,
                parameters.RoomPublicId,
                parameters.CurrentPartyId,
                relationshipCodes)
            .Include(unit => unit.Property)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Loads the minimal room projection for a tenant QR join token.
    /// </summary>
    /// <param name="parameters">The tenant-join room parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tenant-join room row, or <c>null</c>.</returns>
    public Task<TenantJoinRoomRowModel> GetTenantJoinRoomAsync(
        TenantJoinRoomQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // QR preview and confirm need only room identity and capacity fields, not a tracked package/policy graph.
        return _dapperService.QueryFirstOrDefaultAsync<TenantJoinRoomRowModel>(
            InfrastructureQueryConstants.GET_TENANT_JOIN_ROOM_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));
    }

    /// <summary>
    /// Loads tenant-join room state through EF so mutation guards share the active transaction.
    /// </summary>
    /// <param name="parameters">The tenant-join room parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The transaction-bound room state, or <c>null</c>.</returns>
    public async Task<TenantJoinRoomRowModel> GetTenantJoinRoomForMutationAsync(
        TenantJoinRoomQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Resolve the landlord internal identifier without leaving the current EF transaction.
        var landlordPartyId = await _havenDbContext.Parties
            .Where(party => party.PublicId == parameters.LandlordPartyPublicId && !party.IsDeleted)
            .Select(party => (long?)party.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!landlordPartyId.HasValue)
        {
            return null;
        }

        // Step 2: Reuse the landlord-scoped occupancy query so authorization and room state are read transactionally.
        var room = await GetRoomForOccupancyMutationAsync(
            new RoomScopedQueryParametersModel
            {
                CurrentPartyId = landlordPartyId.Value,
                RelationshipCodes = parameters.RelationshipCodes,
                RoomPublicId = parameters.RoomPublicId
            },
            cancellationToken);

        if (room is null)
        {
            return null;
        }

        // Step 3: Return only the fields required by tenant placement and the committed response.
        return new TenantJoinRoomRowModel
        {
            PropertyId = room.PropertyId,
            PropertyPublicId = room.Property.PublicId,
            PropertyName = room.Property.Name,
            UnitId = room.Id,
            RoomPublicId = room.PublicId,
            RoomCode = room.UnitCode,
            RoomName = room.UnitName,
            RentalModeId = room.RentalModeId,
            BedCount = room.BedCount
        };
    }

    /// <summary>
    /// Loads a tracked room by internal identifier for status updates.
    /// </summary>
    /// <param name="unitId">The internal unit identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tracked room, or <c>null</c>.</returns>
    public Task<Unit> GetRoomByIdAsync(
        long unitId,
        CancellationToken cancellationToken = default)
    {
        // Load a tracked room for small status updates inside an existing transaction.
        return _havenDbContext.Units
            .Include(unit => unit.Property)
            .FirstOrDefaultAsync(
                unit => unit.Id == unitId
                        && !unit.IsDeleted,
                cancellationToken);
    }

    /// <summary>
    /// Loads mutation guard counters for the supplied rooms.
    /// </summary>
    /// <param name="unitPublicIds">The room public identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The mutation guard rows.</returns>
    public async Task<IReadOnlyList<UnitMutationGuardModel>> GetRoomMutationGuardsAsync(
        IReadOnlyCollection<Guid> unitPublicIds,
        CancellationToken cancellationToken = default)
    {
        if (unitPublicIds.Count == 0)
        {
            return [];
        }

        // Guards are Dapper reads because contracts, occupancies, billing, and vehicles are outside this EF graph.
        var rows = await _dapperService.QueryAsync<UnitMutationGuardModel>(
            InfrastructureQueryConstants.GET_UNIT_MUTATION_GUARDS_QUERY,
            new { UnitPublicIds = unitPublicIds.ToArray() },
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }
}
