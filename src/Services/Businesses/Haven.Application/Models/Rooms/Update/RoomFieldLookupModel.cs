namespace Haven.Application.Models.Rooms.Update;

/// <summary>
/// Carries resolved master-data identifiers used while applying editable room fields.
/// </summary>
public class RoomFieldLookupModel
{
    /// <summary>
    /// Gets or sets the resolved unit type identifier when the request submits a type code.
    /// </summary>
    public long? UnitTypeId { get; set; }

    /// <summary>
    /// Gets or sets the resolved rental mode identifier when the request submits a rental mode code.
    /// </summary>
    public long? RentalModeId { get; set; }

    /// <summary>
    /// Gets or sets the default room status identifier used only when a new room is created.
    /// </summary>
    public long? AvailableStatusId { get; set; }
}
