namespace Haven.Application.Models.Properties.Create;

/// <summary>
/// Carries values needed to create an item inside a unit package.
/// </summary>
public class PropertyUnitPackageItemBuildModel
{
    /// <summary>
    /// Gets or sets the unit package that owns the item.
    /// </summary>
    public UnitPackage UnitPackage { get; set; }

    /// <summary>
    /// Gets or sets the item display name.
    /// </summary>
    public string ItemName { get; set; }

    /// <summary>
    /// Gets or sets the display order inside the package.
    /// </summary>
    public int DisplayOrder { get; set; }
}
