namespace Haven.Application.Models.Rooms.Update;

/// <summary>
/// Represents editable room fields shared by property-level and room-level update workflows.
/// </summary>
public class RoomFieldUpdateModel
{
    /// <summary>
    /// Gets or sets the floor number when the update moves or creates a room under a floor.
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the room display name when the update changes or creates the room name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the unit type code when the update changes or creates the room type.
    /// </summary>
    public string TypeCode { get; set; }

    /// <summary>
    /// Gets or sets the rental mode code when the update changes or creates the rental mode.
    /// </summary>
    public string RentalModeCode { get; set; }

    /// <summary>
    /// Gets or sets the room area in square meters when supplied.
    /// </summary>
    public decimal? AreaSqm { get; set; }

    /// <summary>
    /// Gets or sets the base rent amount when supplied.
    /// </summary>
    public decimal? BaseRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the default deposit amount when supplied.
    /// </summary>
    public decimal? DefaultDepositAmount { get; set; }

    /// <summary>
    /// Gets or sets the total bed capacity for shared-bed/KTX rooms when supplied.
    /// </summary>
    public int? TotalBeds { get; set; }

    /// <summary>
    /// Gets or sets whether pets are allowed when supplied.
    /// </summary>
    public bool? IsPetAllowed { get; set; }
}
