namespace Haven.Application.Models.Rooms.Delete;

/// <summary>
/// Represents a party-scoped room delete request.
/// </summary>
public class RoomDeleteRequestModel
{
    /// <summary>
    /// Gets or sets the current party context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the room public identifier.
    /// </summary>
    public Guid RoomPublicId { get; set; }
}
