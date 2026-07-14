namespace Haven.Application.Dtos.Properties.Update;

/// <summary>
/// Represents one room submitted by the property edit form.
/// </summary>
public class UpdatePropertyRoomRequestDto
{
    /// <summary>
    /// Gets or sets the frontend-safe room identifier for existing rooms.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the room display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the room/unit type code.
    /// </summary>
    public string TypeCode { get; set; }

    /// <summary>
    /// Gets or sets the rental mode code.
    /// </summary>
    public string RentalModeCode { get; set; }

    /// <summary>
    /// Gets or sets the optional room area in square meters.
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
    /// Gets or sets whether pets are allowed in the room.
    /// </summary>
    public bool? IsPetAllowed { get; set; }
}
