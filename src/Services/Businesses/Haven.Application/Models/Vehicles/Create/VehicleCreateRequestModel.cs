namespace Haven.Application.Models.Vehicles.Create;

/// <summary>
/// Represents the party-scoped vehicle creation input used by the service layer.
/// </summary>
public class VehicleCreateRequestModel
{
    public CurrentPartyContextModel CurrentParty { get; set; }
    public Guid RoomId { get; set; }
    public Guid PayerTenantId { get; set; }
    public string VehicleTypeCode { get; set; }
    public string Name { get; set; }
    public string LicensePlate { get; set; }
    public IFormFile RegistrationFrontImage { get; set; }
    public IFormFile RegistrationSideImage { get; set; }
    public IFormFile VehicleFrontImage { get; set; }
    public IFormFile VehicleSideImage { get; set; }
}
