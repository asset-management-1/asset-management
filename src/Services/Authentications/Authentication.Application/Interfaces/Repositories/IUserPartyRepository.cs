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
}
