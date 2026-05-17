namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for party identifiers.
/// </summary>
public class PartyIdentifierRepository : GenericRepository<PartyIdentifier>, IPartyIdentifierRepository
{
    private readonly AuthenticationDbContext _authenticationDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartyIdentifierRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public PartyIdentifierRepository(AuthenticationDbContext dbContext) : base(dbContext)
    {
        _authenticationDbContext = dbContext;
    }

    /// <summary>
    /// Loads one identifier by type and identifier value.
    /// </summary>
    /// <param name="identifierTypeId">The identifier type master-data value id.</param>
    /// <param name="identifierValue">The normalized identifier value.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched identifier; otherwise <c>null</c>.</returns>
    public Task<PartyIdentifier> GetByTypeAndValueAsync(
        long identifierTypeId,
        string identifierValue,
        CancellationToken cancellationToken = default)
    {
        // Blank identifier values cannot match a stored KYC identifier.
        if (string.IsNullOrWhiteSpace(identifierValue))
        {
            return Task.FromResult<PartyIdentifier>(null);
        }

        // Return a tracked identifier because KYC resubmission may update the same row.
        return _authenticationDbContext.PartyIdentifiers
            .FirstOrDefaultAsync(
                x => !x.IsDeleted
                     && x.IdentifierTypeId == identifierTypeId
                     && x.IdentifierValue == identifierValue,
                cancellationToken);
    }

    /// <summary>
    /// Loads active identifiers of the supplied types for the supplied parties as tracked rows.
    /// </summary>
    /// <param name="partyIds">The linked party ids to inspect.</param>
    /// <param name="identifierTypeIds">The identifier type master-data value ids.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The active tracked identifiers attached to the supplied parties.</returns>
    public async Task<IReadOnlyList<PartyIdentifier>> GetTrackedByPartyIdsAndTypesAsync(
        IReadOnlyCollection<long> partyIds,
        IReadOnlyCollection<long> identifierTypeIds,
        CancellationToken cancellationToken = default)
    {
        // Empty party or type sets mean there is no KYC state to inspect.
        if (partyIds is null || partyIds.Count == 0 || identifierTypeIds is null || identifierTypeIds.Count == 0)
        {
            return [];
        }

        // Load tracked identifier rows so the KYC submit flow can evaluate and update them.
        return await _authenticationDbContext.PartyIdentifiers
            .Where(x => !x.IsDeleted
                        && identifierTypeIds.Contains(x.IdentifierTypeId)
                        && partyIds.Contains(x.PartyId))
            .ToListAsync(cancellationToken);
    }
}
