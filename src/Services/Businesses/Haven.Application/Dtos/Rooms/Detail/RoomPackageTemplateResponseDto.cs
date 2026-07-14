namespace Haven.Application.Dtos.Rooms.Detail;

/// <summary>
/// Represents one effective package template for a room.
/// </summary>
public class RoomPackageTemplateResponseDto
{
    /// <summary>
    /// Gets or sets the package identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the package display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the package price adjustment.
    /// </summary>
    public decimal PriceAdjustment { get; set; }

    /// <summary>
    /// Gets or sets included package items.
    /// </summary>
    public IReadOnlyList<RoomPackageTemplateItemResponseDto> Items { get; set; } = [];
}
