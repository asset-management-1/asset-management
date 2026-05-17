namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides party-specific data access over the authentication database context.
/// </summary>
public class PartyRepository : GenericRepository<Party>, IPartyRepository
{
    private readonly AuthenticationDbContext _authenticationDbContext;

    /// <summary>
    /// Creates the party repository with EF tracking access for profile and context writes.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public PartyRepository(AuthenticationDbContext dbContext) : base(dbContext)
    {
        _authenticationDbContext = dbContext;
    }

    /// <summary>
    /// Loads minimal party-context data by party identifier.
    /// </summary>
    /// <param name="partyId">The party internal identifier to load.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The minimal party context when found; otherwise <c>null</c>.</returns>
    public Task<CurrentPartyContextModel> GetContextByIdAsync(
        long partyId,
        CancellationToken cancellationToken = default)
    {
        // Project only the party type context needed by tenant/landlord switching rules.
        return _authenticationDbContext.Parties
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Id == partyId)
            .ProjectToType<CurrentPartyContextModel>()
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Loads tracked parties by internal identifiers for profile and KYC sync flows.
    /// </summary>
    /// <param name="partyIds">The party internal identifiers to load.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked active parties matched by id.</returns>
    public async Task<IReadOnlyList<Party>> GetTrackedByIdsAsync(
        IEnumerable<long> partyIds,
        CancellationToken cancellationToken = default)
    {
        // De-duplicate ids before querying tracked parties for profile or KYC sync.
        var distinctPartyIds = partyIds?.Distinct().ToList() ?? [];
        if (distinctPartyIds.Count == 0)
        {
            return [];
        }

        // Return tracked party entities because callers update contact/display snapshots.
        return await _authenticationDbContext.Parties
            .Where(x => !x.IsDeleted && distinctPartyIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}
