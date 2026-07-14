namespace Haven.Domain.Entities;

/// <summary>
/// Represents a selectable service or furniture package in asset.UnitPackages.
/// </summary>
public class UnitPackage : BaseEntity
{
    /// <summary>
    /// Gets or sets the internal property identifier that owns this package.
    /// </summary>
    public long PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the parent property navigation.
    /// </summary>
    public Property Property { get; set; }

    /// <summary>
    /// Gets or sets the optional internal unit identifier for room-specific package overrides.
    /// </summary>
    public long? UnitId { get; set; }

    /// <summary>
    /// Gets or sets the optional unit navigation for room-specific package overrides.
    /// </summary>
    public Unit Unit { get; set; }

    /// <summary>
    /// Gets or sets the package type master-data identifier.
    /// </summary>
    public long PackageTypeId { get; set; }

    /// <summary>
    /// Gets or sets the package code unique within the property or overridden room.
    /// </summary>
    public string PackageCode { get; set; }

    /// <summary>
    /// Gets or sets the package display name.
    /// </summary>
    public string PackageName { get; set; }

    /// <summary>
    /// Gets or sets the package description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the amount added to the unit base rent.
    /// </summary>
    public decimal PriceAdjustment { get; set; }

    /// <summary>
    /// Gets or sets the package status master-data identifier.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets the services or furniture items configured inside this package.
    /// </summary>
    public ICollection<UnitPackageItem> Items { get; } = new List<UnitPackageItem>();
}
