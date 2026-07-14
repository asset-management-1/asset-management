namespace Haven.Infrastructure.Models.UnitPackages;

/// <summary>
/// Contains the exact package master-data values required by property and room mutations.
/// </summary>
public sealed class UnitPackageMutationLookupModel
{
    /// <summary>
    /// Gets or sets the default no-furniture package type used for fallback setup.
    /// </summary>
    public MasterDataValueModel NoFurniturePackageType { get; set; }

    /// <summary>
    /// Gets or sets the custom package type identifier.
    /// </summary>
    public long CustomPackageTypeId { get; set; }

    /// <summary>
    /// Gets or sets the active package status identifier.
    /// </summary>
    public long ActiveStatusId { get; set; }
}
