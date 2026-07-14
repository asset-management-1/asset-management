namespace Haven.Application.Dtos.Properties.Create;

/// <summary>
/// Represents one persisted floor summary after property creation.
/// </summary>
public class CreatedPropertyFloorResponseDto
{
    /// <summary>
    /// Gets or sets the submitted floor number.
    /// </summary>
    public int FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the persisted room quantity on this floor.
    /// </summary>
    public int TotalRooms { get; set; }

    /// <summary>
    /// Gets or sets the backend-generated room business codes.
    /// </summary>
    public IReadOnlyList<string> RoomCodes { get; set; } = [];
}

