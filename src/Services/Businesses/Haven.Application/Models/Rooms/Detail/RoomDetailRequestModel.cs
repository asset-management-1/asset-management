namespace Haven.Application.Models.Rooms.Detail;

/// <summary>
/// Represents a party-scoped room detail request.
/// </summary>
public class RoomDetailRequestModel
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
