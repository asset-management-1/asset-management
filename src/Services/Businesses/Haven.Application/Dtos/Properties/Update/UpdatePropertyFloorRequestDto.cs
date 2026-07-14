namespace Haven.Application.Dtos.Properties.Update;

/// <summary>
/// Represents one floor in the property edit structure payload.
/// </summary>
public class UpdatePropertyFloorRequestDto
{
    /// <summary>
    /// Gets or sets the floor number.
    /// </summary>
    public int FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the room rows under this floor.
    /// </summary>
    public IReadOnlyList<UpdatePropertyRoomRequestDto> Rooms { get; set; } = [];
}
