namespace Haven.Infrastructure.Repositories.UnitPackages;

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

    /// <summary>
    /// Replaces all items under the supplied tracked package.
    /// </summary>
    /// <param name="unitPackage">The tracked package whose items should be replaced.</param>
    /// <param name="itemNames">The replacement item names in display order.</param>
    /// <param name="cancellationToken">The token used to cancel the item replacement.</param>
    public async Task ReplaceItemsAsync(
        UnitPackage unitPackage,
        IReadOnlyCollection<string> itemNames,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(unitPackage);

        // Existing rows must be removed first so the package has one item source of truth.
        var existingItems = unitPackage.Items.ToList();

        await DeleteRangeAsync(existingItems);
        unitPackage.Items.Clear();

        // Recreate ordered rows from request-provided names; validators own label quality before persistence.
        var newItems = (itemNames ?? [])
            .Select((name, index) => new UnitPackageItem
            {
                PublicId = Guid.NewGuid(),
                UnitPackage = unitPackage,
                ItemName = name,
                DisplayOrder = index + 1
            })
            .ToList();

        foreach (var newItem in newItems)
        {
            unitPackage.Items.Add(newItem);
        }

        // AddRange accepts empty collections, so the repository can stage the replacement without caller guards.
        await AddRangeAsync(newItems, cancellationToken);
    }
}
