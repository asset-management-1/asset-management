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
    /// Gets the party context selected by one authenticated session.
    /// </summary>
    /// <param name="userPublicId">The authenticated user public identifier.</param>
    /// <param name="sessionPublicId">The authenticated session public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The selected party context, or <c>null</c>.</returns>
    public Task<CurrentPartyContextModel> GetCurrentPartyBySessionAsync(
        Guid userPublicId,
        Guid sessionPublicId,
        CancellationToken cancellationToken = default)
    {
        return _dapperService.QueryFirstOrDefaultAsync<CurrentPartyContextModel>(
            InfrastructureQueryConstants.GET_CURRENT_PARTY_CONTEXT_QUERY,
            new
            {
                UserPublicId = userPublicId,
                SessionPublicId = sessionPublicId
            },
            DapperCommandOptionsHelper.CreateText(cancellationToken));
    }
}
