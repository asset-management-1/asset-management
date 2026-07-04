namespace Haven.Application.Dtos.Properties.List;

/// <summary>
/// Represents the compact room card shown in the property list and quick-room screen.
/// </summary>
public class PropertyListRoomResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe room/unit identifier used to open room actions.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the room/unit code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the room/unit display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the floor number.
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the rental mode code.
    /// </summary>
    public string RentalModeCode { get; set; }

    /// <summary>
    /// Gets or sets the rental mode display name.
    /// </summary>
    public string RentalModeName { get; set; }

    /// <summary>
    /// Gets or sets the room status code.
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the room status display name.
    /// </summary>
    public string StatusName { get; set; }

    /// <summary>
    /// Gets or sets the currently displayed total rent amount.
    /// </summary>
    public decimal TotalRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the total occupied bed/slot quantity.
    /// </summary>
    public int TotalOccupiedBeds { get; set; }

    /// <summary>
    /// Gets or sets total bed/slot capacity for the room.
    /// </summary>
    public int? TotalBeds { get; set; }

    /// <summary>
    /// Gets or sets active tenants and their contract dates for the room card.
    /// </summary>
    public IReadOnlyList<PropertyListRoomTenantResponseDto> Tenants { get; set; } = [];
}
