namespace Haven.Infrastructure.Repositories;

/// <summary>
/// Provides EF write operations for unit packages.
/// </summary>
public class UnitPackageRepository : GenericRepository<UnitPackage>, IUnitPackageRepository
{
    /// <summary>
    /// Creates the unit-package repository.
    /// </summary>
    /// <param name="dbContext">The Haven business database context.</param>
    public UnitPackageRepository(HavenDbContext dbContext) : base(dbContext)
    {
    }
}
