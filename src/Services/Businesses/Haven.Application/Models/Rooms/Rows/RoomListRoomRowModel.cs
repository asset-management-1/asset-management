namespace Haven.Application.Models.Rooms.Rows;

/// <summary>
/// Row model for one room ledger card with property and floor grouping fields.
/// </summary>
public class RoomListRoomRowModel
{
    /// <summary>
    /// Gets or sets the total filtered row count.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the parent property public identifier.
    /// </summary>
    public Guid PropertyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the parent property code.
    /// </summary>
    public string PropertyCode { get; set; }

    /// <summary>
    /// Gets or sets the parent property display name.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the compact parent property address.
    /// </summary>
    public string PropertyAddress { get; set; }

    /// <summary>
    /// Gets or sets the total active rooms in the property.
    /// </summary>
    public int PropertyTotalRooms { get; set; }

    /// <summary>
    /// Gets or sets available rooms in the property.
    /// </summary>
    public int PropertyAvailableRooms { get; set; }

    /// <summary>
    /// Gets or sets the floor number.
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the total active rooms on the floor.
    /// </summary>
    public int FloorTotalRooms { get; set; }

    /// <summary>
    /// Gets or sets the available rooms on the floor.
    /// </summary>
    public int FloorAvailableRooms { get; set; }

    /// <summary>
    /// Gets or sets the room public identifier.
    /// </summary>
    public Guid UnitPublicId { get; set; }

    /// <summary>
    /// Gets or sets the room code.
    /// </summary>
    public string UnitCode { get; set; }

    /// <summary>
    /// Gets or sets the room display name.
    /// </summary>
    public string UnitName { get; set; }

    /// <summary>
    /// Gets or sets the room/unit type code.
    /// </summary>
    public string UnitTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the room/unit type display name.
    /// </summary>
    public string UnitTypeName { get; set; }

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
    /// Gets or sets whether this room uses property-level charge policies.
    /// </summary>
    public bool UsesCommonChargePolicies { get; set; }

    /// <summary>
    /// Gets or sets whether this room uses property-level package templates.
    /// </summary>
    public bool UsesCommonPackages { get; set; }
}
