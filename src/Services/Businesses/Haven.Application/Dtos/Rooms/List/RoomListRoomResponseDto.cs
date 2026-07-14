namespace Haven.Application.Dtos.Rooms.List;

/// <summary>
/// Represents one room card in the room ledger response.
/// </summary>
public class RoomListRoomResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the room code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the room display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the floor number.
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the room/unit type code.
    /// </summary>
    public string TypeCode { get; set; }

    /// <summary>
    /// Gets or sets the room/unit type display name.
    /// </summary>
    public string TypeName { get; set; }

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
    /// Gets or sets the displayed total rent amount.
    /// </summary>
    public decimal TotalRentAmount { get; set; }

    /// <summary>
    /// Gets or sets occupied bed/slot count.
    /// </summary>
    public int TotalOccupiedBeds { get; set; }

    /// <summary>
    /// Gets or sets total bed/slot capacity.
    /// </summary>
    public int? TotalBeds { get; set; }

    /// <summary>
    /// Gets or sets whether the room uses property-level charge policies.
    /// </summary>
    public bool UsesCommonChargePolicies { get; set; }

    /// <summary>
    /// Gets or sets whether the room uses property-level package templates.
    /// </summary>
    public bool UsesCommonPackages { get; set; }

    /// <summary>
    /// Gets or sets active tenants and contract dates for the room.
    /// </summary>
    public IReadOnlyList<RoomListTenantResponseDto> Tenants { get; set; } = [];
}
