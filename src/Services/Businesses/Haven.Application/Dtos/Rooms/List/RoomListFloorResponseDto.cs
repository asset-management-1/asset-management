namespace Haven.Application.Dtos.Rooms.List;

/// <summary>
/// Represents one floor group in the room ledger response.
/// </summary>
public class RoomListFloorResponseDto
{
    /// <summary>
    /// Gets or sets the floor number.
    /// </summary>
    public int? Number { get; set; }

    /// <summary>
    /// Gets or sets the total active rooms on the floor.
    /// </summary>
    public int TotalRooms { get; set; }

    /// <summary>
    /// Gets or sets the available rooms on the floor.
    /// </summary>
    public int AvailableRooms { get; set; }

    /// <summary>
    /// Gets or sets rooms included in the current page for this floor.
    /// </summary>
    public IReadOnlyList<RoomListRoomResponseDto> Rooms { get; set; } = [];
}
