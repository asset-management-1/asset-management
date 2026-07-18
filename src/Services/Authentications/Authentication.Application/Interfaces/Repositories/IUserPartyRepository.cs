namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Defines user-party-specific repository operations.
/// </summary>
public interface IUserPartyRepository : IGenericRepository<UserParty>
{
    /// <summary>
    /// Loads all active user-party links for a user with party-type data.
    /// </summary>
    Task<IReadOnlyList<UserParty>> GetActiveByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads one active user-party link by user identifier and target party context.
    /// </summary>
    Task<UserParty> GetByUserIdAndPartyTypeAsync(long userId, string partyType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads all active user-party links matching one Party type in deterministic creation order.
    /// </summary>
    Task<IReadOnlyList<UserParty>> GetAllByUserIdAndPartyTypeAsync(
        long userId,
        string partyType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the Party context to retain or select deterministically for one client session.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="currentPartyId">The Party currently selected by the client session, if any.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The valid current Party, the earliest active Party, or <c>null</c> when no active Party exists.</returns>
    Task<long?> ResolveSessionPartyIdAsync(
        long userId,
        long? currentPartyId,
        CancellationToken cancellationToken = default);
}
