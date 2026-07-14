namespace Haven.Application.Models.Vehicles.Update;

/// <summary>
/// Represents the party-scoped partial vehicle update input used by the service layer.
/// </summary>
public class VehicleUpdateRequestModel
{
    public CurrentPartyContextModel CurrentParty { get; set; }
    public Guid RoomId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid? PayerTenantId { get; set; }
    public string VehicleTypeCode { get; set; }
    public string Name { get; set; }
    public string LicensePlate { get; set; }
    public IFormFile RegistrationFrontImage { get; set; }
    public IFormFile RegistrationSideImage { get; set; }
    public IFormFile VehicleFrontImage { get; set; }
    public IFormFile VehicleSideImage { get; set; }
}
