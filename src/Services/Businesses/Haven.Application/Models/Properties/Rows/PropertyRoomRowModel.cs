namespace Haven.Application.Models.Properties.Rows;

/// <summary>
/// Row model for a room/unit card.
/// </summary>
public class PropertyRoomRowModel
{
    /// <summary>
    /// Gets or sets the parent property public identifier.
    /// </summary>
    public Guid PropertyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the public unit identifier.
    /// </summary>
    public Guid UnitPublicId { get; set; }

    /// <summary>
    /// Gets or sets the unit code.
    /// </summary>
    public string UnitCode { get; set; }

    /// <summary>
    /// Gets or sets the unit display name.
    /// </summary>
    public string UnitName { get; set; }

    /// <summary>
    /// Gets or sets the floor number.
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the room area in square meters.
    /// </summary>
    public decimal? AreaSqm { get; set; }

    /// <summary>
    /// Gets or sets the base rent amount.
    /// </summary>
    public decimal BaseRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the default deposit amount configured for the room.
    /// </summary>
    public decimal? DefaultDepositAmount { get; set; }

    /// <summary>
    /// Gets or sets the total rent amount shown on the room card.
    /// </summary>
    public decimal TotalRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the bed/slot count.
    /// </summary>
    public int? BedCount { get; set; }

    /// <summary>
    /// Gets or sets whether pets are allowed.
    /// </summary>
    public bool IsPetAllowed { get; set; }

    /// <summary>
    /// Gets or sets the unit type code.
    /// </summary>
    public string UnitTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the unit type display name.
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
    /// Gets or sets the unit status code.
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the unit status display name.
    /// </summary>
    public string StatusName { get; set; }

    /// <summary>
    /// Gets or sets the latest payment status code.
    /// </summary>
    public string PaymentStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the latest payment status display name.
    /// </summary>
    public string PaymentStatusName { get; set; }

    /// <summary>
    /// Gets or sets active occupied bed/slot count.
    /// </summary>
    public int OccupiedBedCount { get; set; }

    /// <summary>
    /// Gets or sets whether this room currently uses property-level charge policies.
    /// </summary>
    public bool UsesCommonChargePolicies { get; set; }

    /// <summary>
    /// Gets or sets whether this room currently uses property-level package templates.
    /// </summary>
    public bool UsesCommonPackages { get; set; }

}

