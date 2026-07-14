namespace Haven.Api.Models.Meters;

/// <summary>
/// Extends the shared room meter form with evidence that an existing period explicitly removes.
/// </summary>
public sealed class UpdateRoomMeterForm : RoomMeterForm
{
    /// <summary>
    /// Gets or sets selected electricity evidence image identifiers to remove.
    /// </summary>
    public List<Guid> DeletedElectricImageIds { get; set; } = [];

    /// <summary>
    /// Gets or sets selected water evidence image identifiers to remove.
    /// </summary>
    public List<Guid> DeletedWaterImageIds { get; set; } = [];
}
