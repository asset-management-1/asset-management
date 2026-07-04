namespace Haven.Application.Dtos.Properties.Create;

/// <summary>
/// Represents a room service/furniture package template created during property setup.
/// </summary>
public class CreatePropertyPackageRequestDto
{
    /// <summary>
    /// Gets or sets the package display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the amount added to the room base rent when this package is selected.
    /// </summary>
    public decimal PriceAdjustment { get; set; }

    /// <summary>
    /// Gets or sets the items included in the package.
    /// </summary>
    public IReadOnlyList<CreatePropertyPackageItemRequestDto> Items { get; set; } = [];
}
