namespace Haven.Application.Models.Rooms.Rows;

/// <summary>
/// Row model for one room detail header.
/// </summary>
public class RoomDetailRowModel
{
    /// <summary>
    /// Gets or sets the room internal identifier.
    /// </summary>
    public long UnitId { get; set; }

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
    /// Gets or sets the floor number.
    /// </summary>
    public int? FloorNumber { get; set; }

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
    /// Gets or sets the room area in square meters.
    /// </summary>
    public decimal? AreaSqm { get; set; }

    /// <summary>
    /// Gets or sets the base rent amount.
    /// </summary>
    public decimal BaseRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the default deposit amount.
    /// </summary>
    public decimal DefaultDepositAmount { get; set; }

    /// <summary>
    /// Gets or sets total bed/slot capacity.
    /// </summary>
    public int? TotalBeds { get; set; }

    /// <summary>
    /// Gets or sets whether pets are allowed.
    /// </summary>
    public bool IsPetAllowed { get; set; }

    /// <summary>
    /// Gets or sets whether this room uses property-level charge policies.
    /// </summary>
    public bool UsesCommonChargePolicies { get; set; }

    /// <summary>
    /// Gets or sets whether this room uses property-level package templates.
    /// </summary>
    public bool UsesCommonPackages { get; set; }
}
