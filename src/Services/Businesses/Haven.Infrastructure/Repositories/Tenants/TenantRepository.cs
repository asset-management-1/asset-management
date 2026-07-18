namespace Haven.Infrastructure.Repositories.Tenants;

/// <summary>
/// Provides tenant read models and EF write graph operations.
/// </summary>
public class TenantRepository : ITenantRepository
{
    private readonly HavenDbContext _havenDbContext;
    private readonly IDapperService _dapperService;

    /// <summary>
    /// Creates the tenant repository.
    /// </summary>
    /// <param name="dbContext">The Haven business database context.</param>
    /// <param name="dapperService">The shared Dapper service.</param>
    public TenantRepository(
        HavenDbContext dbContext,
        IDapperService dapperService)
    {
        _havenDbContext = dbContext;
        _dapperService = dapperService;
    }

    /// <summary>
    /// Loads a paged tenant ledger read model.
    /// </summary>
    /// <param name="parameters">The tenant list query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tenant rows.</returns>
    public async Task<IReadOnlyList<TenantRowModel>> GetTenantPageAsync(
        TenantListQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // Dapper projects only ledger fields and carries the filtered window count on each row.
        var rows = await _dapperService.QueryAsync<TenantRowModel>(
            InfrastructureQueryConstants.GET_TENANT_LIST_PAGE_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Loads one tenant occupancy detail row in the current party scope.
    /// </summary>
    /// <param name="parameters">The scoped tenant query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tenant row, or <c>null</c>.</returns>
    public Task<TenantRowModel> GetTenantDetailAsync(
        TenantScopedQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // The SQL scope hides occupancies outside the current landlord relationship.
        return _dapperService.QueryFirstOrDefaultAsync<TenantRowModel>(
            InfrastructureQueryConstants.GET_TENANT_DETAIL_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));
    }

    /// <summary>
    /// Loads one active tenant party by public identifier.
    /// </summary>
    /// <param name="tenantPublicId">The tenant party public identifier.</param>
    /// <param name="tenantTypeId">The tenant party type identifier.</param>
    /// <param name="activeStatusId">The active party status identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tenant party, or <c>null</c>.</returns>
    public Task<Party> GetTenantPartyAsync(
        Guid tenantPublicId,
        long tenantTypeId,
        long activeStatusId,
        CancellationToken cancellationToken = default)
    {
        // Keep tracking enabled because the resolved party may join a new occupancy in the same unit of work.
        return _havenDbContext.Parties.FirstOrDefaultAsync(
            party => party.PublicId == tenantPublicId
                     && party.PartyTypeId == tenantTypeId
                     && party.StatusId == activeStatusId
                     && !party.IsDeleted,
            cancellationToken);
    }

    /// <summary>
    /// Loads one user account with linked tenant parties.
    /// </summary>
    /// <param name="accountPublicId">The user account public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The user account graph, or <c>null</c>.</returns>
    public Task<User> GetUserWithTenantPartiesAsync(
        Guid accountPublicId,
        CancellationToken cancellationToken = default)
    {
        // Load account-party links as-is; service resolves tenant-role ambiguity using already-resolved master-data ids.
        return _havenDbContext.Users
            .Include(user => user.UserParties.Where(link => !link.IsDeleted))
                .ThenInclude(link => link.Party)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                user => user.PublicId == accountPublicId
                        && !user.IsDeleted,
                cancellationToken);
    }

    /// <summary>
    /// Loads one occupancy for landlord-scoped move-out.
    /// </summary>
    /// <param name="occupancyPublicId">The occupancy public identifier.</param>
    /// <param name="currentPartyId">The current landlord party identifier.</param>
    /// <param name="activeStatusId">The active occupancy status identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The occupancy graph, or <c>null</c>.</returns>
    public Task<Occupancy> GetOccupancyForMoveOutAsync(
        Guid occupancyPublicId,
        long currentPartyId,
        long activeStatusId,
        CancellationToken cancellationToken = default)
    {
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Load only an active occupancy and its room so every move-out uses the shared room mutation lock.
        return _havenDbContext.Occupancies
            .Include(occupancy => occupancy.Contract)
            .Include(occupancy => occupancy.Unit)
            .FirstOrDefaultAsync(
                occupancy => occupancy.PublicId == occupancyPublicId
                             && occupancy.StatusId == activeStatusId
                             && !occupancy.IsDeleted
                             && _havenDbContext.PropertyParties.Any(
                                 party => party.PropertyId == occupancy.PropertyId
                                          && party.PartyId == currentPartyId
                                          && !party.IsDeleted
                                          && (party.StartDate == null || party.StartDate <= currentDate)
                                          && (party.EndDate == null || party.EndDate >= currentDate)),
                cancellationToken);
    }

    /// <summary>
    /// Loads the room public identifier for an active landlord-scoped occupancy.
    /// </summary>
    /// <param name="occupancyPublicId">The occupancy public identifier.</param>
    /// <param name="currentPartyId">The current landlord party identifier.</param>
    /// <param name="activeStatusId">The active occupancy status identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room public identifier, or <c>null</c>.</returns>
    public Task<Guid?> GetMoveOutRoomPublicIdAsync(
        Guid occupancyPublicId,
        long currentPartyId,
        long activeStatusId,
        CancellationToken cancellationToken = default)
    {
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // This no-tracking projection selects the shared room lock without polluting the later tracked mutation query.
        return _havenDbContext.Occupancies
            .AsNoTracking()
            .Where(occupancy => occupancy.PublicId == occupancyPublicId
                                && occupancy.StatusId == activeStatusId
                                && !occupancy.IsDeleted
                                && _havenDbContext.PropertyParties.Any(
                                    party => party.PropertyId == occupancy.PropertyId
                                             && party.PartyId == currentPartyId
                                             && !party.IsDeleted
                                             && (party.StartDate == null || party.StartDate <= currentDate)
                                             && (party.EndDate == null || party.EndDate >= currentDate)))
            .Select(occupancy => (Guid?)occupancy.Unit.PublicId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Checks whether a contract code already exists.
    /// </summary>
    /// <param name="contractCode">The generated contract code.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns><c>true</c> when the contract code exists.</returns>
    public Task<bool> ContractCodeExistsAsync(
        string contractCode,
        CancellationToken cancellationToken = default)
    {
        // Generated contract codes are checked against every historical row, including soft-deleted contracts.
        return _havenDbContext.Contracts.AnyAsync(
            contract => contract.ContractCode == contractCode,
            cancellationToken);
    }

    /// <summary>
    /// Counts primary occupancies in selected lifecycle statuses for one room.
    /// </summary>
    /// <param name="unitId">The internal room identifier.</param>
    /// <param name="statusIds">The occupancy status identifiers included in capacity usage.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The primary occupancy count.</returns>
    public Task<int> CountPrimaryOccupanciesAsync(
        long unitId,
        IReadOnlyCollection<long> statusIds,
        CancellationToken cancellationToken = default)
    {
        // An empty lifecycle set consumes no room capacity and avoids an unnecessary database round trip.
        if (statusIds is null || statusIds.Count == 0)
        {
            return Task.FromResult(0);
        }

        // Pending and active status ids are supplied by the caller so the same query protects each entry flow.
        return _havenDbContext.Occupancies.CountAsync(
            occupancy => occupancy.UnitId == unitId
                         && statusIds.Contains(occupancy.StatusId)
                         && occupancy.IsPrimaryTenant
                         && !occupancy.IsDeleted,
            cancellationToken);
    }

    /// <summary>
    /// Checks whether a tenant already has an occupancy in selected lifecycle statuses for a room.
    /// </summary>
    /// <param name="unitId">The internal room identifier.</param>
    /// <param name="partyId">The internal tenant party identifier.</param>
    /// <param name="statusIds">The occupancy status identifiers considered duplicates.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns><c>true</c> when a matching occupancy exists.</returns>
    public Task<bool> HasOccupancyAsync(
        long unitId,
        long partyId,
        IReadOnlyCollection<long> statusIds,
        CancellationToken cancellationToken = default)
    {
        // Without lifecycle statuses there is no duplicate state to inspect.
        if (statusIds is null || statusIds.Count == 0)
        {
            return Task.FromResult(false);
        }

        // Restrict duplicate detection to the room, tenant party, and caller-approved lifecycle states.
        return _havenDbContext.Occupancies.AnyAsync(
            occupancy => occupancy.UnitId == unitId
                         && occupancy.PartyId == partyId
                         && statusIds.Contains(occupancy.StatusId)
                         && !occupancy.IsDeleted,
            cancellationToken);
    }

    /// <summary>
    /// Counts active occupancies in one room.
    /// </summary>
    /// <param name="unitId">The internal room identifier.</param>
    /// <param name="activeStatusId">The active occupancy status identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The active occupancy count.</returns>
    public Task<int> CountActiveOccupanciesAsync(
        long unitId,
        long activeStatusId,
        CancellationToken cancellationToken = default)
    {
        // Count current residents only; historical move-outs remain queryable but do not consume occupancy.
        return _havenDbContext.Occupancies.CountAsync(
            occupancy => occupancy.UnitId == unitId
                         && occupancy.StatusId == activeStatusId
                         && !occupancy.IsDeleted,
            cancellationToken);
    }

    /// <summary>
    /// Soft-deletes active vehicles attached to a party in one room.
    /// </summary>
    /// <param name="unitId">The internal room identifier.</param>
    /// <param name="partyId">The tenant party identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    public async Task SoftDeleteVehiclesAsync(
        long unitId,
        long partyId,
        CancellationToken cancellationToken = default)
    {
        // Load only active vehicles billed to this tenant in this room before staging soft deletion.
        var vehicles = await _havenDbContext.PartyVehicles
            .Where(vehicle => vehicle.UnitId == unitId
                              && vehicle.PartyId == partyId
                              && !vehicle.IsDeleted)
            .ToListAsync(cancellationToken);

        // Preserve vehicle and invoice history while excluding the records from future billing.
        foreach (var vehicle in vehicles)
        {
            vehicle.IsDeleted = true;
        }
    }

    /// <summary>
    /// Stages a new tenant party and its account link.
    /// </summary>
    /// <param name="party">The tenant party entity.</param>
    /// <param name="userParty">The User-Party relationship entity.</param>
    /// <param name="cancellationToken">The token used to cancel the insert.</param>
    public async Task AddTenantPartyLinkAsync(
        Party party,
        UserParty userParty,
        CancellationToken cancellationToken = default)
    {
        // Account-backed tenant creation owns both rows, so they are staged together.
        await _havenDbContext.Parties.AddAsync(party, cancellationToken);
        await _havenDbContext.UserParties.AddAsync(userParty, cancellationToken);
    }

    /// <summary>
    /// Stages a tenant occupancy and its optional contract.
    /// </summary>
    /// <param name="occupancy">The occupancy entity.</param>
    /// <param name="contract">The optional rental contract entity.</param>
    /// <param name="cancellationToken">The token used to cancel the insert.</param>
    public async Task AddTenantOccupancyAsync(
        Occupancy occupancy,
        Contract contract,
        CancellationToken cancellationToken = default)
    {
        // Primary tenants may own a contract; non-contract occupants stage only the occupancy row.
        if (contract is not null)
        {
            await _havenDbContext.Contracts.AddAsync(contract, cancellationToken);
        }

        await _havenDbContext.Occupancies.AddAsync(occupancy, cancellationToken);
    }
}
