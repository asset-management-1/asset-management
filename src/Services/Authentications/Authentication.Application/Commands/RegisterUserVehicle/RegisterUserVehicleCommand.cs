namespace Authentication.Application.Commands.RegisterUserVehicle;

/// <summary>
/// Represents a request to register a vehicle under the current tenant profile.
/// </summary>
public class RegisterUserVehicleCommand : ICommand<ResponseDto<UserVehicleResponseDto>>
{
    /// <summary>
    /// Gets or sets the vehicle type. Accepted values: <c>Car</c>, <c>Motorbike</c>, <c>Bicycle</c>.
    /// </summary>
    public VehicleTypeEnum VehicleType { get; set; }

    /// <summary>
    /// Gets or sets the vehicle display name.
    /// </summary>
    public string VehicleName { get; set; }

    /// <summary>
    /// Gets or sets the vehicle license plate value.
    /// </summary>
    public string LicensePlate { get; set; }

    /// <summary>
    /// Gets or sets the optional front-side vehicle image. Send as a binary multipart file part.
    /// </summary>
    public IFormFile FrontFile { get; set; }

    /// <summary>
    /// Gets or sets the optional side vehicle image. Send as a binary multipart file part.
    /// </summary>
    public IFormFile SideFile { get; set; }
}
