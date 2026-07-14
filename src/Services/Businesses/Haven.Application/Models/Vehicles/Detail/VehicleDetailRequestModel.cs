namespace Haven.Application.Models.Vehicles.Detail;

/// <summary>
/// Represents a party-scoped vehicle detail request.
/// </summary>
public class VehicleDetailRequestModel
{
    public CurrentPartyContextModel CurrentParty { get; set; }
    public Guid RoomPublicId { get; set; }
    public Guid VehiclePublicId { get; set; }
}
