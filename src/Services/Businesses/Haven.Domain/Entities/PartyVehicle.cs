namespace Haven.Domain.Entities;

/// <summary>
/// Represents a vehicle attached to a room and paying party in core.PartyVehicles.
/// </summary>
public class PartyVehicle : BaseEntity
{
    /// <summary>
    /// Gets or sets the paying party identifier.
    /// </summary>
    public long PartyId { get; set; }

    /// <summary>
    /// Gets or sets the attached room/unit identifier.
    /// </summary>
    public long UnitId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type master-data identifier.
    /// </summary>
    public long VehicleTypeId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle display name.
    /// </summary>
    public string VehicleName { get; set; }

    /// <summary>
    /// Gets or sets the vehicle license plate.
    /// </summary>
    public string LicensePlate { get; set; }

    /// <summary>
    /// Gets or sets the optional registration-document front image URL.
    /// </summary>
    public string RegistrationFrontImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the optional registration-document side image URL.
    /// </summary>
    public string RegistrationSideImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the optional vehicle front image URL.
    /// </summary>
    public string VehicleFrontImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the optional vehicle side image URL.
    /// </summary>
    public string VehicleSideImageUrl { get; set; }
}
