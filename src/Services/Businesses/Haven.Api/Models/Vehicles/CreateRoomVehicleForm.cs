namespace Haven.Api.Models.Vehicles;

/// <summary>
/// Represents multipart fields submitted when a vehicle is registered inside a routed room.
/// </summary>
public sealed class CreateRoomVehicleForm
{
    /// <summary>
    /// Gets or sets the primary tenant who pays the vehicle charge.
    /// </summary>
    public Guid PayerTenantId { get; set; }

    /// <summary>
    /// Gets or sets the selected vehicle-type master-data code.
    /// </summary>
    public string VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the optional license plate.
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
