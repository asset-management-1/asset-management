namespace Haven.Application.Interfaces.Repositories;

/// <summary>
/// Provides write repository operations for units.
/// </summary>
public interface IUnitRepository : IGenericRepository<Unit>
{
    /// <summary>
    /// Gets one page of room ledger rows.
    /// </summary>
    /// <param name="parameters">The room list query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room ledger rows.</returns>
    Task<IReadOnlyList<RoomListRoomRowModel>> GetRoomPageAsync(
        RoomListQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active tenant rows for selected rooms.
    /// </summary>
    /// <param name="unitPublicIds">The room public identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room tenant rows.</returns>
    Task<IReadOnlyList<RoomTenantRowModel>> GetRoomTenantsAsync(
        IReadOnlyCollection<Guid> unitPublicIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one room detail row scoped by current party.
    /// </summary>
    /// <param name="parameters">The scoped room query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room detail row, or <c>null</c>.</returns>
    Task<RoomDetailRowModel> GetRoomDetailAsync(
        RoomScopedQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets effective charge policies for one room.
    /// </summary>
    /// <param name="parameters">The landlord-scoped room query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The effective charge policy rows.</returns>
    Task<IReadOnlyList<RoomChargePolicyRowModel>> GetEffectiveChargePoliciesAsync(
        Guid roomPublicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets effective package templates for one room.
    /// </summary>
    /// <param name="parameters">The landlord-scoped room and fallback-package query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The effective package template rows.</returns>
    Task<IReadOnlyList<RoomPackageTemplateRowModel>> GetEffectivePackageTemplatesAsync(
        RoomPackageQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Locks and loads the tracked package graph for one room inside the current transaction.
    /// </summary>
    /// <param name="parameters">The landlord-scoped room query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the lock and graph load.</param>
    /// <returns>The tracked room, common packages, and room override packages, or <c>null</c>.</returns>
    Task<Unit> GetRoomPackageGraphForMutationAsync(
        RoomScopedQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a property already has a unit business code.
    /// </summary>
    /// <param name="propertyId">The internal property identifier.</param>
    /// <param name="unitCode">The backend-generated unit business code.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns><c>true</c> when the unit code already exists under the property.</returns>
    Task<bool> ExistsUnitCodeAsync(
        long propertyId,
        string unitCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tracked room graph scoped by current party.
    /// </summary>
    /// <param name="parameters">The scoped room query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tracked unit graph, or <c>null</c>.</returns>
    Task<Unit> GetRoomGraphAsync(
        RoomScopedQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the minimal room projection scoped by a tenant-join QR payload.
    /// </summary>
    /// <param name="parameters">The tenant-join room scope parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tenant-join room row, or <c>null</c>.</returns>
    Task<TenantJoinRoomRowModel> GetTenantJoinRoomAsync(
        TenantJoinRoomQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tracked room by internal identifier.
    /// </summary>
    /// <param name="unitId">The room internal identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tracked room, or <c>null</c>.</returns>
    Task<Unit> GetRoomByIdAsync(
        long unitId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets protected dependency counters for selected rooms.
    /// </summary>
    /// <param name="unitPublicIds">The room public identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room mutation guard counters.</returns>
    Task<IReadOnlyList<UnitMutationGuardModel>> GetRoomMutationGuardsAsync(
        IReadOnlyCollection<Guid> unitPublicIds,
        CancellationToken cancellationToken = default);
}
