namespace Haven.Infrastructure.Repositories.UnitPackages;

/// <summary>
/// Provides EF write operations for unit packages.
/// </summary>
public class UnitPackageRepository : GenericRepository<UnitPackage>, IUnitPackageRepository
{
    private readonly HavenDbContext _havenDbContext;

    /// <summary>
    /// Creates the unit-package repository.
    /// </summary>
    /// <param name="dbContext">The Haven business database context.</param>
    public UnitPackageRepository(HavenDbContext dbContext) : base(dbContext)
    {
        _havenDbContext = dbContext;
    }

    /// <summary>
    /// Checks whether an active package code already exists in one room.
    /// </summary>
    public Task<bool> ExistsPackageCodeAsync(
        long unitId,
        string packageCode,
        CancellationToken cancellationToken = default)
    {
        return _havenDbContext.UnitPackages.AsNoTracking().AnyAsync(
            package => package.UnitId == unitId
                       && package.PackageCode == packageCode
                       && !package.IsDeleted,
            cancellationToken);
    }

    /// <summary>
    /// Checks whether a non-deleted contract references one package.
    /// </summary>
    public Task<bool> HasContractReferenceAsync(
        long unitPackageId,
        CancellationToken cancellationToken = default)
    {
        return _havenDbContext.Contracts.AsNoTracking().AnyAsync(
            contract => contract.UnitPackageId == unitPackageId && !contract.IsDeleted,
            cancellationToken);
    }

    /// <summary>
    /// Soft-deletes the supplied packages and their loaded item rows.
    /// </summary>
    /// <param name="unitPackages">The tracked unit packages to delete.</param>
    public void DeletePackagesWithItems(IReadOnlyCollection<UnitPackage> unitPackages)
    {
        // Package deletion owns its child item cleanup so service workflows do not duplicate this persistence detail.
        var packages = (unitPackages ?? []).ToList();
        var items = packages
            .SelectMany(unitPackage => unitPackage.Items)
            .ToList();

        // Preserve package and item history while removing both levels from future effective-package reads.
        foreach (var item in items)
        {
            item.IsDeleted = true;
        }

        foreach (var package in packages)
        {
            package.IsDeleted = true;
        }

        _dbContext.Set<UnitPackageItem>().UpdateRange(items);
        _dbContext.Set<UnitPackage>().UpdateRange(packages);
    }
}
