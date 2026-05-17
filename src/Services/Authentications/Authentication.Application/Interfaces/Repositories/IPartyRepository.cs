namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Defines party-specific repository operations.
/// </summary>
public interface IPartyRepository : IGenericRepository<Party>
{
    /// <summary>
    /// Loads minimal party-context data by party identifier.
    /// </summary>
    /// <param name="partyId">The party internal identifier to load.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The minimal party context when found; otherwise <c>null</c>.</returns>
    Task<CurrentPartyContextModel> GetContextByIdAsync(
        long partyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads tracked parties by internal identifiers for profile and KYC sync flows.
    /// </summary>
    /// <param name="partyIds">The party internal identifiers to load.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked active parties matched by id.</returns>
    Task<IReadOnlyList<Party>> GetTrackedByIdsAsync(
        IEnumerable<long> partyIds,
        CancellationToken cancellationToken = default);
}
