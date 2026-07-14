namespace Haven.Api.Models.Vehicles;

/// <summary>
/// Represents multipart fields submitted for a partial vehicle update inside a routed room.
/// </summary>
public sealed class UpdateRoomVehicleForm
{
    /// <summary>
    /// Gets or sets an optional replacement primary tenant payer.
    /// </summary>
    public Guid? PayerTenantId { get; set; }

    /// <summary>
    /// Gets or sets an optional replacement vehicle-type master-data code.
    /// </summary>
    public string VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets an optional replacement vehicle display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets an optional replacement license plate.
    /// </summary>
    public string LicensePlate { get; set; }

    /// <summary>
    /// Gets or sets an optional replacement registration front image.
    /// </summary>
    public IFormFile RegistrationFrontImage { get; set; }

    /// <summary>
    /// Gets or sets an optional replacement registration side image.
    /// </summary>
    public IFormFile RegistrationSideImage { get; set; }

    /// <summary>
    /// Gets or sets an optional replacement vehicle front image.
    /// </summary>
    public IFormFile VehicleFrontImage { get; set; }

    /// <summary>
    /// Gets or sets an optional replacement vehicle side image.
    /// </summary>
    public IFormFile VehicleSideImage { get; set; }
}
