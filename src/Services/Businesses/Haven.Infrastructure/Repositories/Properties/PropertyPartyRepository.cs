namespace Haven.Infrastructure.Repositories.Properties;

/// <summary>
/// Provides EF write operations for property-party links.
/// </summary>
public class PropertyPartyRepository : GenericRepository<PropertyParty>, IPropertyPartyRepository
{
    /// <summary>
    /// Creates the property-party repository.
    /// </summary>
    /// <param name="dbContext">The Haven business database context.</param>
    public PropertyPartyRepository(HavenDbContext dbContext) : base(dbContext)
    {
    }
}
