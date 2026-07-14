namespace Haven.Application.Models.Tenants.Rows;

/// <summary>
/// Row model for room QR join preview data.
/// </summary>
public class TenantJoinRoomRowModel
{
    /// <summary>
    /// Gets or sets the internal property identifier used by occupancy persistence.
    /// </summary>
    public long PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the property public identifier.
    /// </summary>
    public Guid PropertyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the property display name.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the internal room identifier used by occupancy persistence.
    /// </summary>
    public long UnitId { get; set; }

    /// <summary>
    /// Gets or sets the room public identifier.
    /// </summary>
    public Guid RoomPublicId { get; set; }

    /// <summary>
    /// Gets or sets the room code.
    /// </summary>
    public string RoomCode { get; set; }

    /// <summary>
    /// Gets or sets the room display name.
    /// </summary>
    public string RoomName { get; set; }

    /// <summary>
    /// Gets or sets the room rental-mode master-data identifier used by capacity rules.
    /// </summary>
    public long RentalModeId { get; set; }

    /// <summary>
    /// Gets or sets the configured bed capacity for shared-bed rooms.
    /// </summary>
    public int? BedCount { get; set; }
}
