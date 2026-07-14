namespace Haven.Application.Interfaces.Repositories;

/// <summary>
/// Provides write repository operations for unit package items.
/// </summary>
public interface IUnitPackageItemRepository : IGenericRepository<UnitPackageItem>
{
    /// <summary>
    /// Replaces all items under the supplied tracked package.
    /// </summary>
    /// <param name="unitPackage">The tracked package whose items should be replaced.</param>
    /// <param name="itemNames">The replacement item names in display order.</param>
    /// <param name="cancellationToken">The token used to cancel the item replacement.</param>
    Task ReplaceItemsAsync(
        UnitPackage unitPackage,
        IReadOnlyCollection<string> itemNames,
        CancellationToken cancellationToken);

}
