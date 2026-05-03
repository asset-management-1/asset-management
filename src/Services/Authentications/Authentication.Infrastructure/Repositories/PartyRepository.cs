namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides party-specific data access over the authentication database context.
/// </summary>
public class PartyRepository : GenericRepository<Party>, IPartyRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PartyRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public PartyRepository(AuthenticationDbContext dbContext) : base(dbContext)
    {
    }
}
