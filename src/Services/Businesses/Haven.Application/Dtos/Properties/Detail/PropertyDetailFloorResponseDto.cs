namespace Haven.Application.Dtos.Properties.Detail;

/// <summary>
/// Represents one floor in the building detail structure section.
/// </summary>
public class PropertyDetailFloorResponseDto
{
    /// <summary>
    /// Gets or sets the floor number.
    /// </summary>
    public int? Number { get; set; }

    /// <summary>
    /// Gets or sets the rooms on this floor.
    /// </summary>
    public IReadOnlyList<PropertyDetailRoomResponseDto> Rooms { get; set; } = [];
}
