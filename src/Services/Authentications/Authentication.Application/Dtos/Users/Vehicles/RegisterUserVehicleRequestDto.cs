namespace Authentication.Application.Dtos.Users.Vehicles;

/// <summary>
/// Represents the tenant profile vehicle registration payload used by infrastructure.
/// </summary>
public class RegisterUserVehicleRequestDto
{
    /// <summary>
    /// Gets or sets the vehicle type code.
    /// </summary>
    public string VehicleType { get; set; }

    /// <summary>
    /// Gets or sets the vehicle display name.
    /// </summary>
    public string VehicleName { get; set; }

    /// <summary>
    /// Gets or sets the vehicle license plate value.
    /// </summary>
    public string LicensePlate { get; set; }

    /// <summary>
    /// Gets or sets the optional front-side vehicle image.
    /// </summary>
    public IFormFile FrontFile { get; set; }

    /// <summary>
    /// Gets or sets the optional side vehicle image.
    /// </summary>
    public IFormFile SideFile { get; set; }
}
