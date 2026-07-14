namespace Haven.Application.Models.Properties.Rows;

/// <summary>
/// Row model for package templates configured on units under one property.
/// </summary>
public class PropertyPackageTemplateRowModel
{
    /// <summary>
    /// Gets or sets the package public identifier.
    /// </summary>
    public Guid PackagePublicId { get; set; }

    /// <summary>
    /// Gets or sets the package display name.
    /// </summary>
    public string PackageName { get; set; }

    /// <summary>
    /// Gets or sets the package price adjustment.
    /// </summary>
    public decimal PriceAdjustment { get; set; }

    /// <summary>
    /// Gets or sets one item name from the package, when available.
    /// </summary>
    public string ItemName { get; set; }

    /// <summary>
    /// Gets or sets the item display order.
    /// </summary>
    public int? DisplayOrder { get; set; }
}
