namespace Haven.Application.Commands.CreateVehicle;

/// <summary>
/// Represents a landlord multipart request to register one vehicle for a room.
/// </summary>
public class CreateVehicleCommand : ICommand<ResponseDto<VehicleDetailResponseDto>>
{
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
