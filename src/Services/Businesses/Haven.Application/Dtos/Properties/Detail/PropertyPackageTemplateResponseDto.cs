namespace Haven.Application.Dtos.Properties.Detail;

/// <summary>
/// Represents a package template configured across rooms in a property.
/// </summary>
public class PropertyPackageTemplateResponseDto
{
    /// <summary>
    /// Gets or sets the package code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the package display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the package price adjustment.
    /// </summary>
    public decimal PriceAdjustment { get; set; }

    /// <summary>
    /// Gets or sets the included package items.
    /// </summary>
    public IReadOnlyList<PropertyPackageTemplateItemResponseDto> Items { get; set; } = [];
}
