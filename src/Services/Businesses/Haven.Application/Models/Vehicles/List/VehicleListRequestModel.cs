namespace Haven.Application.Models.Vehicles.List;

/// <summary>
/// Represents a landlord-scoped vehicle collection for one room.
/// </summary>
public sealed class VehicleListRequestModel
{
    /// <summary>
    /// Gets or sets the current landlord context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the required room identifier.
    /// </summary>
    public Guid RoomId { get; set; }
}
