namespace Haven.Application.Interfaces.Repositories;

/// <summary>
/// Provides party read operations.
/// </summary>
public interface IPartyRepository
{
    /// <summary>
    /// Gets the authenticated user's selected party context.
    /// </summary>
    /// <param name="userPublicId">The authenticated user public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The selected party context, or <c>null</c>.</returns>
    Task<CurrentPartyContextModel> GetCurrentPartyByUserPublicIdAsync(
        Guid userPublicId,
        CancellationToken cancellationToken = default);
}
