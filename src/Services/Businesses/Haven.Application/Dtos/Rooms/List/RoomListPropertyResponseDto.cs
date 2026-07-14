namespace Haven.Application.Dtos.Rooms.List;

/// <summary>
/// Represents one property group in the room ledger response.
/// </summary>
public class RoomListPropertyResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe property identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the property code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the property display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the compact property address.
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// Gets or sets the total active room count in the property.
    /// </summary>
    public int TotalRooms { get; set; }

    /// <summary>
    /// Gets or sets the available room count in the property.
    /// </summary>
    public int AvailableRooms { get; set; }

    /// <summary>
    /// Gets or sets floor groups included in the current page.
    /// </summary>
    public IReadOnlyList<RoomListFloorResponseDto> Floors { get; set; } = [];
}
