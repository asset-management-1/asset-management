namespace Haven.Application.Models.Vehicles.Rows;

/// <summary>
/// Represents the persistence projection needed by the vehicle detail response.
/// </summary>
public class VehicleDetailRowModel
{
    public Guid VehiclePublicId { get; set; }
    public Guid RoomPublicId { get; set; }
    public string RoomName { get; set; }
    public Guid PayerTenantPublicId { get; set; }
    public string PayerTenantName { get; set; }
    public string VehicleTypeCode { get; set; }
    public string VehicleTypeName { get; set; }
    public string VehicleName { get; set; }
    public string LicensePlate { get; set; }
    public string RegistrationFrontImageUrl { get; set; }
    public string RegistrationSideImageUrl { get; set; }
    public string VehicleFrontImageUrl { get; set; }
    public string VehicleSideImageUrl { get; set; }
}
