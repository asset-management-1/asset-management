namespace Haven.Application.Commands.UpdateVehicle;

/// <summary>
/// Represents a partial multipart request to update one vehicle.
/// </summary>
public class UpdateVehicleCommand : ICommand<ResponseDto<VehicleDetailResponseDto>>
{
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
