namespace Haven.Application.Dtos.Properties.Create;

/// <summary>
/// Represents one explicit room sent by the property setup UI.
/// </summary>
public class CreatePropertyRoomRequestDto
{
    /// <summary>
    /// Gets or sets the room code supplied by the UI.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the room display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the unit type code.
    /// </summary>
    public string TypeCode { get; set; }

    /// <summary>
    /// Gets or sets the rental mode code.
    /// </summary>
    public string RentalModeCode { get; set; }

    /// <summary>
    /// Gets or sets the optional area in square meters.
    /// </summary>
    public decimal? AreaSqm { get; set; }

    /// <summary>
    /// Gets or sets the base rent amount.
    /// </summary>
    public decimal? BaseRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the default deposit amount.
    /// </summary>
    public decimal? DefaultDepositAmount { get; set; }

    /// <summary>
    /// Gets or sets the total bed/slot quantity for shared-bed rooms.
    /// </summary>
    public int? TotalBeds { get; set; }

    /// <summary>
    /// Gets or sets whether pets are allowed in this room.
    /// </summary>
    public bool IsPetAllowed { get; set; }
}
