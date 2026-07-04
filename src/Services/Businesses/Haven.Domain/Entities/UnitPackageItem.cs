namespace Haven.Domain.Entities;

/// <summary>
/// Represents one service or furniture item inside an asset.UnitPackages package.
/// </summary>
public class UnitPackageItem : BaseEntity
{
    /// <summary>
    /// Gets or sets the internal unit package identifier.
    /// </summary>
    public long UnitPackageId { get; set; }

    /// <summary>
    /// Gets or sets the unit package navigation.
    /// </summary>
    public UnitPackage UnitPackage { get; set; }

    /// <summary>
    /// Gets or sets the item display name.
    /// </summary>
    public string ItemName { get; set; }

    /// <summary>
    /// Gets or sets the display order within the package.
    /// </summary>
    public int DisplayOrder { get; set; }
}
