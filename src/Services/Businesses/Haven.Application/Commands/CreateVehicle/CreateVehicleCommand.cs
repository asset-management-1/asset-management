namespace Haven.Application.Commands.CreateVehicle;

/// <summary>
/// Represents a landlord multipart request to register one vehicle for a room.
/// </summary>
public class CreateVehicleCommand : ICommand<ResponseDto<VehicleDetailResponseDto>>
{
    /// <summary>
    /// Gets or sets the owning room identifier.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Gets or sets the payer tenant identifier.
    /// </summary>
    public Guid PayerTenantId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle-type code.
    /// </summary>
    public string VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the license plate.
    /// </summary>
    public string LicensePlate { get; set; }

    /// <summary>
    /// Gets or sets the registration front image.
    /// </summary>
    public IFormFile RegistrationFrontImage { get; set; }

    /// <summary>
    /// Gets or sets the registration side image.
    /// </summary>
    public IFormFile RegistrationSideImage { get; set; }

    /// <summary>
    /// Gets or sets the vehicle front image.
    /// </summary>
    public IFormFile VehicleFrontImage { get; set; }

    /// <summary>
    /// Gets or sets the vehicle side image.
    /// </summary>
    public IFormFile VehicleSideImage { get; set; }
}
