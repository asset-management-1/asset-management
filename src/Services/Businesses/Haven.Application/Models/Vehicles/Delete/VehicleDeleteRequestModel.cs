namespace Haven.Application.Models.Vehicles.Delete;

/// <summary>
/// Represents a party-scoped vehicle delete request.
/// </summary>
public class VehicleDeleteRequestModel
{
    public CurrentPartyContextModel CurrentParty { get; set; }
    public Guid RoomPublicId { get; set; }
    public Guid VehiclePublicId { get; set; }
}
