namespace Haven.Application.Models.Rooms.Rows;

/// <summary>
/// Row model for one effective room package template item row.
/// </summary>
public class RoomPackageTemplateRowModel
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
    /// Gets or sets one package item name.
    /// </summary>
    public string ItemName { get; set; }

    /// <summary>
    /// Gets or sets the package item display order.
    /// </summary>
    public int? DisplayOrder { get; set; }
}
