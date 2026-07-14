namespace Haven.Application.Dtos.Rooms.Detail;

/// <summary>
/// Represents editable room fields plus compact parent property context.
/// </summary>
public class RoomDetailBasicInfoResponseDto
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
    /// Gets or sets the parent property identifier.
    /// </summary>
    public Guid PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the parent property code.
    /// </summary>
    public string PropertyCode { get; set; }

    /// <summary>
    /// Gets or sets the parent property display name.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the parent property compact address.
    /// </summary>
    public string PropertyAddress { get; set; }

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
    /// Gets or sets whether the room uses property-level charge policies.
    /// </summary>
    public bool UsesCommonChargePolicies { get; set; }

    /// <summary>
    /// Gets or sets whether the room uses property-level package templates.
    /// </summary>
    public bool UsesCommonPackages { get; set; }
}
