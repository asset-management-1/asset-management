namespace Haven.Application.Dtos.Properties.Create;

/// <summary>
/// Represents one explicit floor sent by the property setup UI.
/// </summary>
public class CreatePropertyFloorRequestDto
{
    /// <summary>
    /// Gets or sets the floor number.
    /// </summary>
    public int FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets explicit rooms on this floor.
    /// </summary>
    public IReadOnlyList<CreatePropertyRoomRequestDto> Rooms { get; set; } = [];
}
