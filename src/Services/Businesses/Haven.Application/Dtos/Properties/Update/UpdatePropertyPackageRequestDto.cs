namespace Haven.Application.Dtos.Properties.Update;

/// <summary>
/// Represents one package template submitted by the property edit form.
/// </summary>
public class UpdatePropertyPackageRequestDto
{
    /// <summary>
    /// Gets or sets the package identifier for an existing package template.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the package display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the amount added to the room base rent when selected.
    /// </summary>
    public decimal? PriceAdjustment { get; set; }

    /// <summary>
    /// Gets or sets the package items.
    /// </summary>
    public IReadOnlyList<UpdatePropertyPackageItemRequestDto> Items { get; set; }
}
