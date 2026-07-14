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
            .Where(x => !x.IsDeleted && x.UserId == userId)
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
                     && x.Party.PartyType.Code == partyType)
            .Select(x => new UserParty
            {
                Id = x.Id,
                UserId = x.UserId,
                PartyId = x.PartyId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
