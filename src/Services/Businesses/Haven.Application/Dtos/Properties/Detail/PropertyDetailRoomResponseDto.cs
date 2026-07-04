namespace Haven.Application.Dtos.Properties.Detail;

/// <summary>
/// Represents an edit-summary room under a building detail.
/// </summary>
public class PropertyDetailRoomResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe room/unit identifier.
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
    /// Gets or sets the rental mode code.
    /// </summary>
    public string RentalModeCode { get; set; }

    /// <summary>
    /// Gets or sets the rental mode display name.
    /// </summary>
    public string RentalModeName { get; set; }

    /// <summary>
    /// Gets or sets the room/unit type code.
    /// </summary>
    public string TypeCode { get; set; }

    /// <summary>
    /// Gets or sets the room/unit type display name.
    /// </summary>
    public string TypeName { get; set; }

    /// <summary>
    /// Gets or sets the unit status code.
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the unit status display name.
    /// </summary>
    public string StatusName { get; set; }

    /// <summary>
    /// Gets or sets the room base rent amount.
    /// </summary>
    public decimal BaseRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the room default deposit amount.
    /// </summary>
    public decimal? DefaultDepositAmount { get; set; }

    /// <summary>
    /// Gets or sets the room area in square meters.
    /// </summary>
    public decimal? AreaSqm { get; set; }

    /// <summary>
    /// Gets or sets the total bed/slot quantity for shared-bed rooms.
    /// </summary>
    public int? TotalBeds { get; set; }

    /// <summary>
    /// Gets or sets whether pets are allowed in the room.
    /// </summary>
    public bool IsPetAllowed { get; set; }
}
