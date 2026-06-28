namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Defines repository operations for tenant profile vehicles.
/// </summary>
public interface IPartyVehicleRepository : IGenericRepository<PartyVehicle>
{
    /// <summary>
    /// Loads active vehicles for one tenant party.
    /// </summary>
    /// <param name="partyId">The tenant party internal identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The active profile vehicles registered under the tenant party.</returns>
    Task<IReadOnlyList<UserVehicleResponseDto>> GetActiveByPartyIdAsync(
        long partyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a tracked vehicle by public id while enforcing tenant-party ownership.
    /// </summary>
    /// <param name="vehiclePublicId">The public vehicle identifier supplied by the API.</param>
    /// <param name="partyId">The tenant party internal identifier that must own the vehicle.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked active vehicle when found; otherwise <c>null</c>.</returns>
    Task<PartyVehicle> GetTrackedByPublicIdAndPartyIdAsync(
        Guid vehiclePublicId,
        long partyId,
        CancellationToken cancellationToken = default);
}
