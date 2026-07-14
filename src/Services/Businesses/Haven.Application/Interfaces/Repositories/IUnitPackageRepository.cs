namespace Haven.Application.Interfaces.Repositories;

/// <summary>
/// Provides write repository operations for unit packages.
/// </summary>
public interface IUnitPackageRepository : IGenericRepository<UnitPackage>
{
    /// <summary>
    /// Checks whether a package code already exists in the selected room scope.
    /// </summary>
    /// <param name="unitId">The internal room identifier.</param>
    /// <param name="packageCode">The backend-generated package code.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns><c>true</c> when the package code already exists in the room.</returns>
    Task<bool> ExistsPackageCodeAsync(
        long unitId,
        string packageCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether any non-deleted contract references the selected package.
    /// </summary>
    /// <param name="unitPackageId">The internal room-package identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns><c>true</c> when a contract references the package.</returns>
    Task<bool> HasContractReferenceAsync(
        long unitPackageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the supplied packages and their loaded item rows.
    /// </summary>
    /// <param name="unitPackages">The tracked unit packages to delete.</param>
    void DeletePackagesWithItems(IReadOnlyCollection<UnitPackage> unitPackages);
}
