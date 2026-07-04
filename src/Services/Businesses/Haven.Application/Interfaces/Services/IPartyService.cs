namespace Haven.Application.Interfaces.Services;

/// <summary>
/// Resolves the authenticated user's party context for Haven business flows.
/// </summary>
public interface IPartyService
{
    /// <summary>
    /// Gets the current party context or throws when no authenticated party can be used.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The current party context.</returns>
    Task<CurrentPartyContextModel> GetCurrentPartyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current party and ensures it is an active landlord context.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The current active landlord party context.</returns>
    Task<CurrentPartyContextModel> GetCurrentLandlordAsync(CancellationToken cancellationToken = default);
}
