namespace Haven.Application.Models.Vehicles.Detail;

/// <summary>
/// Represents a party-scoped vehicle detail request.
/// </summary>
public class VehicleDetailRequestModel
{
    /// <summary>
    /// Gets or sets the current party context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the public owning room identifier.
    /// </summary>
    public Guid RoomPublicId { get; set; }

    /// <summary>
    /// Gets or sets the public vehicle identifier.
    /// </summary>
    public Guid VehiclePublicId { get; set; }
}
