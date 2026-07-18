namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for user-party mappings.
/// </summary>
public class UserPartyRepository : GenericRepository<UserParty>, IUserPartyRepository
{
    private readonly AuthenticationDbContext _authenticationDbContext;

    /// <summary>
    /// Creates the user-party repository with EF access for account context mappings.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public UserPartyRepository(AuthenticationDbContext dbContext) : base(dbContext)
    {
        _authenticationDbContext = dbContext;
    }

    /// <summary>
    /// Loads all active party links for a user, including the party and party type.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The active user-party mappings for the user.</returns>
    public async Task<IReadOnlyList<UserParty>> GetActiveByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        // Load minimal active mappings plus party type data for context selection.
        return await _authenticationDbContext.UserParties
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                        && x.UserId == userId
                        && !x.Party.IsDeleted
                        && !x.Party.PartyType.IsDeleted
                        && x.Party.PartyType.IsActive
                        && !x.Party.Status.IsDeleted
                        && x.Party.Status.IsActive
                        && x.Party.Status.Code == ACTIVE_STATUS
                        && x.Party.Status.MasterDataType.Code == PARTY_STATUS_TYPE)
            .Select(x => new UserParty
            {
                Id = x.Id,
                UserId = x.UserId,
                PartyId = x.PartyId,
                Party = new Party
                {
                    Id = x.Party.Id,
                    PartyType = new MasterDataValue
                    {
                        Id = x.Party.PartyType.Id,
                        Code = x.Party.PartyType.Code
                    }
                }
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Loads one active user-party mapping by user and party-type code.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="partyType">The canonical party-type code, such as TENANT or LANDLORD.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user-party mapping; otherwise <c>null</c>.</returns>
    public Task<UserParty> GetByUserIdAndPartyTypeAsync(long userId, string partyType, CancellationToken cancellationToken = default)
    {
        // Missing party type input cannot resolve to a user-party context.
        if (string.IsNullOrWhiteSpace(partyType))
        {
            return Task.FromResult<UserParty>(null);
        }

        // Project only mapping ids because switch-party only needs to activate the existing party.
        return _authenticationDbContext.UserParties
            .AsNoTracking()
            .Where(
                x => !x.IsDeleted
                     && x.UserId == userId
                     && !x.Party.IsDeleted
                     && !x.Party.PartyType.IsDeleted
                     && x.Party.PartyType.IsActive
                     && !x.Party.Status.IsDeleted
                     && x.Party.Status.IsActive
                     && x.Party.Status.Code == ACTIVE_STATUS
                     && x.Party.Status.MasterDataType.Code == PARTY_STATUS_TYPE
                     && x.Party.PartyType.Code == partyType)
            .Select(x => new UserParty
            {
                Id = x.Id,
                UserId = x.UserId,
                PartyId = x.PartyId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Loads every active Party relation of one type for the specified user.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="partyType">The canonical Party type code.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matching active User-Party relations.</returns>
    public async Task<IReadOnlyList<UserParty>> GetAllByUserIdAndPartyTypeAsync(
        long userId,
        string partyType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(partyType))
        {
            return [];
        }

        // Order matching relations so a session without a retained context selects the same Party every time.
        return await _authenticationDbContext.UserParties
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                        && x.UserId == userId
                        && !x.Party.IsDeleted
                        && !x.Party.PartyType.IsDeleted
                        && x.Party.PartyType.IsActive
                        && !x.Party.Status.IsDeleted
                        && x.Party.Status.IsActive
                        && x.Party.Status.Code == ACTIVE_STATUS
                        && x.Party.Status.MasterDataType.Code == PARTY_STATUS_TYPE
                        && x.Party.PartyType.Code == partyType)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => new UserParty
            {
                Id = x.Id,
                UserId = x.UserId,
                PartyId = x.PartyId
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Resolves the valid Party context retained by a client session.
    /// </summary>
    /// <param name="userId">The internal user identifier that owns the session.</param>
    /// <param name="currentPartyId">The Party currently retained by the session, when present.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The retained Party identifier, the earliest active Party identifier, or <c>null</c> when none exists.</returns>
    public async Task<long?> ResolveSessionPartyIdAsync(
        long userId,
        long? currentPartyId,
        CancellationToken cancellationToken = default)
    {
        // Load only valid Party identifiers because token issuance does not need an entity graph.
        var partyIds = await _authenticationDbContext.UserParties
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                        && x.UserId == userId
                        && !x.Party.IsDeleted
                        && !x.Party.PartyType.IsDeleted
                        && x.Party.PartyType.IsActive
                        && !x.Party.PartyType.MasterDataType.IsDeleted
                        && x.Party.PartyType.MasterDataType.Code == PARTY_TYPE_TYPE
                        && !x.Party.Status.IsDeleted
                        && x.Party.Status.IsActive
                        && !x.Party.Status.MasterDataType.IsDeleted
                        && x.Party.Status.Code == ACTIVE_STATUS
                        && x.Party.Status.MasterDataType.Code == PARTY_STATUS_TYPE)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => x.PartyId)
            .ToListAsync(cancellationToken);

        // Preserve an existing selection while its UserParty relation remains valid.
        if (currentPartyId.HasValue && partyIds.Contains(currentPartyId.Value))
        {
            return currentPartyId;
        }

        // A missing or stale context falls back deterministically so user-info always has one active Party.
        return partyIds.Count > 0
            ? partyIds[0]
            : null;
    }
}
