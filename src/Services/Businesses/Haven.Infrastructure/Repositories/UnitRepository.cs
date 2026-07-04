namespace Haven.Infrastructure.Repositories;

/// <summary>
/// Provides EF write operations for units.
/// </summary>
public class UnitRepository : GenericRepository<Unit>, IUnitRepository
{
    /// <summary>
    /// Creates the unit repository.
    /// </summary>
    /// <param name="dbContext">The Haven business database context.</param>
    public UnitRepository(HavenDbContext dbContext) : base(dbContext)
    {
    }
}
