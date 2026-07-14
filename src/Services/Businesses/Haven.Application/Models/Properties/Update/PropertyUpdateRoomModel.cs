namespace Haven.Application.Models.Properties.Update;

/// <summary>
/// Represents one edited room with its parent floor number.
/// </summary>
public class PropertyUpdateRoomModel
{
    /// <summary>
    /// Gets or sets the parent floor number.
    /// </summary>
    public int FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the edited room payload.
    /// </summary>
    public UpdatePropertyRoomRequestDto Room { get; set; }

    /// <summary>
    /// Gets or sets the flattened scalar fields prepared once for guard and mutation phases.
    /// </summary>
    public RoomFieldUpdateModel Fields { get; set; }

    /// <summary>
    /// Gets or sets the master-data identifiers resolved for the submitted room fields.
    /// </summary>
    public RoomFieldLookupModel Lookups { get; set; }
}
