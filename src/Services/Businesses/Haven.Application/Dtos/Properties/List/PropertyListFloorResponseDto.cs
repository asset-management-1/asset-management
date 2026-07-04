namespace Haven.Application.Dtos.Properties.List;

/// <summary>
/// Represents one floor group on the landlord property list screen.
/// </summary>
public class PropertyListFloorResponseDto
{
    /// <summary>
    /// Gets or sets the floor number derived from Units.FloorNumber.
    /// </summary>
    public int? Number { get; set; }

    /// <summary>
    /// Gets or sets the total room quantity on the floor.
    /// </summary>
    public int TotalRooms { get; set; }

    /// <summary>
    /// Gets or sets the available room count on the floor.
    /// </summary>
    public int AvailableRooms { get; set; }

    /// <summary>
    /// Gets or sets the room cards for the Figma quick-room screen.
    /// </summary>
    public IReadOnlyList<PropertyListRoomResponseDto> Rooms { get; set; } = [];
}
