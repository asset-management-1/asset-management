namespace Haven.Application.Dtos.Properties.Create;

/// <summary>
/// Represents one generated floor summary after property creation.
/// </summary>
public class CreatedPropertyFloorResponseDto
{
    /// <summary>
    /// Gets or sets the generated floor number.
    /// </summary>
    public int FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the generated total room quantity on this floor.
    /// </summary>
    public int TotalRooms { get; set; }

    /// <summary>
    /// Gets or sets generated room codes.
    /// </summary>
    public IReadOnlyList<string> RoomCodes { get; set; } = [];
}

