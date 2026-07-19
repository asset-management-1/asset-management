namespace Haven.Application.Commands.UpdateVehicle;

/// <summary>
/// Represents a partial multipart request to update one vehicle.
/// </summary>
public class UpdateVehicleCommand : ICommand<ResponseDto<VehicleDetailResponseDto>>
{
    /// <summary>
    /// Gets or sets the owning room identifier.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle identifier.
    /// </summary>
    public Guid VehicleId { get; set; }

    /// <summary>
    /// Gets or sets the optional replacement payer tenant identifier.
    /// </summary>
    public Guid? PayerTenantId { get; set; }

    /// <summary>
    /// Gets or sets the optional replacement vehicle-type code.
    /// </summary>
    public string VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the optional replacement vehicle name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the optional replacement license plate.
    /// </summary>
    public string LicensePlate { get; set; }

    /// <summary>
    /// Gets or sets the optional registration front image.
    /// </summary>
    public IFormFile RegistrationFrontImage { get; set; }

    /// <summary>
    /// Gets or sets the optional registration side image.
    /// </summary>
    public IFormFile RegistrationSideImage { get; set; }

    /// <summary>
    /// Gets or sets the optional vehicle front image.
    /// </summary>
    public IFormFile VehicleFrontImage { get; set; }

    /// <summary>
    /// Gets or sets the optional vehicle side image.
    /// </summary>
    public IFormFile VehicleSideImage { get; set; }
}
