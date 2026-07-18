namespace Haven.Application.Interfaces.Repositories;

/// <summary>
/// Provides party read operations.
/// </summary>
public interface IPartyRepository
{
    /// <summary>
    /// Gets the party context selected by one authenticated session.
    /// </summary>
    /// <param name="userPublicId">The authenticated user public identifier.</param>
    /// <param name="sessionPublicId">The authenticated session public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The selected party context, or <c>null</c>.</returns>
    Task<CurrentPartyContextModel> GetCurrentPartyBySessionAsync(
        Guid userPublicId,
        Guid sessionPublicId,
        CancellationToken cancellationToken = default);
}
