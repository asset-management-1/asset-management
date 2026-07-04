namespace Haven.Infrastructure.Repositories;

/// <summary>
/// Provides EF write operations for unit package items.
/// </summary>
public class UnitPackageItemRepository : GenericRepository<UnitPackageItem>, IUnitPackageItemRepository
{
    /// <summary>
    /// Creates the unit-package-item repository.
    /// </summary>
    /// <param name="dbContext">The Haven business database context.</param>
    public UnitPackageItemRepository(HavenDbContext dbContext) : base(dbContext)
    {
    }
}
