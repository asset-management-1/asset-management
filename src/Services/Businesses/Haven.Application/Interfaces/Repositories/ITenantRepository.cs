namespace Haven.Application.Interfaces.Repositories;

/// <summary>
/// Provides tenant read and write persistence operations.
/// </summary>
public interface ITenantRepository
{
    /// <summary>
    /// Gets one page of tenant occupancy rows.
    /// </summary>
    /// <param name="parameters">The list query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tenant rows.</returns>
    Task<IReadOnlyList<TenantRowModel>> GetTenantPageAsync(
        TenantListQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one tenant occupancy row.
    /// </summary>
    /// <param name="parameters">The scoped query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tenant row, or <c>null</c>.</returns>
    Task<TenantRowModel> GetTenantDetailAsync(
        TenantScopedQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an existing tenant party by public identifier.
    /// </summary>
    /// <param name="tenantPublicId">The tenant party public identifier.</param>
    /// <param name="tenantTypeId">The tenant party type identifier.</param>
    /// <param name="activeStatusId">The active party status identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tenant party, or <c>null</c>.</returns>
    Task<Party> GetTenantPartyAsync(
        Guid tenantPublicId,
        long tenantTypeId,
        long activeStatusId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one account and linked tenant parties by account public identifier.
    /// </summary>
    /// <param name="accountPublicId">The user account public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tracked user account, or <c>null</c>.</returns>
    Task<User> GetUserWithTenantPartiesAsync(
        Guid accountPublicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one occupancy graph scoped to the current landlord.
    /// </summary>
    /// <param name="occupancyPublicId">The occupancy public identifier.</param>
    /// <param name="currentPartyId">The current landlord party identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tracked occupancy, or <c>null</c>.</returns>
    Task<Occupancy> GetOccupancyForMoveOutAsync(
        Guid occupancyPublicId,
        long currentPartyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a contract code already exists.
    /// </summary>
    /// <param name="contractCode">The contract code candidate.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns><c>true</c> when the code exists.</returns>
    Task<bool> ContractCodeExistsAsync(
        string contractCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts primary occupancies in the supplied lifecycle statuses for one room.
    /// </summary>
    /// <param name="unitId">The room internal identifier.</param>
    /// <param name="statusIds">The occupancy status identifiers included in capacity usage.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The primary occupancy count.</returns>
    Task<int> CountPrimaryOccupanciesAsync(
        long unitId,
        IReadOnlyCollection<long> statusIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether one tenant already has an occupancy in the supplied lifecycle statuses for a room.
    /// </summary>
    /// <param name="unitId">The internal room identifier.</param>
    /// <param name="partyId">The internal tenant party identifier.</param>
    /// <param name="statusIds">The occupancy status identifiers considered duplicates.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns><c>true</c> when a matching occupancy already exists.</returns>
    Task<bool> HasOccupancyAsync(
        long unitId,
        long partyId,
        IReadOnlyCollection<long> statusIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts active occupancies for one room.
    /// </summary>
    /// <param name="unitId">The room internal identifier.</param>
    /// <param name="activeStatusId">The active occupancy status identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The active occupancy count.</returns>
    Task<int> CountActiveOccupanciesAsync(
        long unitId,
        long activeStatusId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes active vehicles linked to one room and tenant party.
    /// </summary>
    /// <param name="unitId">The room internal identifier.</param>
    /// <param name="partyId">The tenant party internal identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the update.</param>
    /// <returns>A task representing the operation.</returns>
    Task SoftDeleteVehiclesAsync(
        long unitId,
        long partyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a new tenant party and its account link.
    /// </summary>
    /// <param name="party">The tenant party to add.</param>
    /// <param name="userParty">The account-party link to add.</param>
    /// <param name="cancellationToken">The token used to cancel the insert.</param>
    /// <returns>A task representing the operation.</returns>
    Task AddTenantPartyLinkAsync(
        Party party,
        UserParty userParty,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a tenant occupancy and its optional contract.
    /// </summary>
    /// <param name="occupancy">The occupancy to add.</param>
    /// <param name="contract">The optional contract to add.</param>
    /// <param name="cancellationToken">The token used to cancel the insert.</param>
    /// <returns>A task representing the operation.</returns>
    Task AddTenantOccupancyAsync(
        Occupancy occupancy,
        Contract contract,
        CancellationToken cancellationToken = default);
}
