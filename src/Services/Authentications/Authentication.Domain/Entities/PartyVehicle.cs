namespace Authentication.Domain.Entities;

/// <summary>
/// Represents a vehicle registered on a tenant party profile.
/// </summary>
public class PartyVehicle : BaseEntity
{
    /// <summary>
    /// Gets or sets the tenant party that owns this profile vehicle.
    /// </summary>
    public long PartyId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type master-data value id.
    /// </summary>
    public long VehicleTypeId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle display name supplied by the tenant.
    /// </summary>
    public string VehicleName { get; set; }

    /// <summary>
    /// Gets or sets the license plate value as entered for display.
    /// </summary>
    public string LicensePlate { get; set; }

    /// <summary>
    /// Gets or sets the normalized license plate used for tenant-scoped duplicate checks.
    /// </summary>
    public string NormalizedLicensePlate { get; set; }

    /// <summary>
    /// Gets or sets the public URL or object path for the vehicle front image.
    /// </summary>
    public string FrontImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the public URL or object path for the vehicle side image.
    /// </summary>
    public string SideImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the tenant party that owns this vehicle.
    /// </summary>
    public virtual Party Party { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type master-data value.
    /// </summary>
    public virtual MasterDataValue VehicleType { get; set; }
}
