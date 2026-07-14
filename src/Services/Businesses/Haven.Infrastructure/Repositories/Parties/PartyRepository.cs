namespace Haven.Infrastructure.Repositories.Parties;

/// <summary>
/// Provides party persistence reads.
/// </summary>
public class PartyRepository : IPartyRepository
{
    private readonly IDapperService _dapperService;

    /// <summary>
    /// Creates the party repository.
    /// </summary>
    /// <param name="dapperService">The shared Dapper service.</param>
    public PartyRepository(IDapperService dapperService)
    {
        _dapperService = dapperService;
    }

    /// <summary>
    /// Gets the authenticated user's selected party context.
    /// </summary>
    /// <param name="userPublicId">The authenticated user public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The selected party context, or <c>null</c>.</returns>
    public Task<CurrentPartyContextModel> GetCurrentPartyByUserPublicIdAsync(
        Guid userPublicId,
        CancellationToken cancellationToken = default)
    {
        return _dapperService.QueryFirstOrDefaultAsync<CurrentPartyContextModel>(
            InfrastructureQueryConstants.GET_CURRENT_PARTY_CONTEXT_QUERY,
            new { UserPublicId = userPublicId },
            DapperCommandOptionsHelper.CreateText(cancellationToken));
    }
}
