namespace Haven.Application.Interfaces.Repositories;

/// <summary>
/// Provides vehicle persistence and scoped lookup operations.
/// </summary>
public interface IVehicleRepository : IGenericRepository<PartyVehicle>
{
    /// <summary>
    /// Loads all active vehicle rows for a landlord-scoped room.
    /// </summary>
    /// <param name="parameters">The landlord and room scope.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The ordered compact vehicle rows.</returns>
    Task<IReadOnlyList<VehicleListRowModel>> GetByRoomAsync(
        VehicleListQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads one vehicle detail row inside the current landlord scope.
    /// </summary>
    /// <param name="parameters">The vehicle identifier and current landlord scope.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The vehicle detail row, or <c>null</c>.</returns>
    Task<VehicleDetailRowModel> GetDetailAsync(
        VehicleScopeQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads one tracked vehicle in the current landlord property scope.
    /// </summary>
    /// <param name="parameters">The vehicle identifier and current landlord scope.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The tracked vehicle, or <c>null</c>.</returns>
    Task<PartyVehicle> GetForMutationAsync(
        VehicleScopeQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a primary tenant with an active rental contract in one room.
    /// </summary>
    /// <param name="parameters">The room, payer, landlord scope, and active-contract criteria.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The resolved room and payer identifiers, or <c>null</c>.</returns>
    Task<VehiclePayerRowModel> GetPrimaryPayerAsync(
        VehiclePayerEligibilityQueryParametersModel parameters,
        CancellationToken cancellationToken = default);

}
