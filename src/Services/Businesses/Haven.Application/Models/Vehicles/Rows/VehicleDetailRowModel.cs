namespace Haven.Application.Models.Vehicles.Rows;

/// <summary>
/// Represents the persistence projection needed by the vehicle detail response.
/// </summary>
public class VehicleDetailRowModel
{
    /// <summary>
    /// Gets or sets the public vehicle identifier.
    /// </summary>
    public Guid VehiclePublicId { get; set; }

    /// <summary>
    /// Gets or sets the public room identifier.
    /// </summary>
    public Guid RoomPublicId { get; set; }

    /// <summary>
    /// Gets or sets the room display name.
    /// </summary>
    public string RoomName { get; set; }

    /// <summary>
    /// Gets or sets the public payer tenant identifier.
    /// </summary>
    public Guid PayerTenantPublicId { get; set; }

    /// <summary>
    /// Gets or sets the payer tenant display name.
    /// </summary>
    public string PayerTenantName { get; set; }

    /// <summary>
    /// Gets or sets the vehicle-type code.
    /// </summary>
    public string VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle-type display name.
    /// </summary>
    public string VehicleTypeName { get; set; }

    /// <summary>
    /// Gets or sets the vehicle display name.
    /// </summary>
    public string VehicleName { get; set; }

    /// <summary>
    /// Gets or sets the license plate.
    /// </summary>
    public string LicensePlate { get; set; }

    /// <summary>
    /// Gets or sets the registration front image URL.
    /// </summary>
    public string RegistrationFrontImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the registration side image URL.
    /// </summary>
    public string RegistrationSideImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the vehicle front image URL.
    /// </summary>
    public string VehicleFrontImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the vehicle side image URL.
    /// </summary>
    public string VehicleSideImageUrl { get; set; }
}
