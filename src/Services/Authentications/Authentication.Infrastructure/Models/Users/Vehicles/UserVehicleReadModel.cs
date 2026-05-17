namespace Authentication.Infrastructure.Models.Users.Vehicles;

/// <summary>
/// Represents raw vehicle data read before mapping internal codes to public enum contracts.
/// </summary>
public sealed class UserVehicleReadModel
{
    /// <summary>
    /// Gets or sets the public vehicle identifier used by API clients.
    /// </summary>
    public Guid PublicId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type code or name from master data.
    /// </summary>
    public string VehicleType { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type display name.
    /// </summary>
    public string VehicleTypeDisplayName { get; set; }

    /// <summary>
    /// Gets or sets the vehicle display name.
    /// </summary>
    public string VehicleName { get; set; }

    /// <summary>
    /// Gets or sets the vehicle license plate value.
    /// </summary>
    public string LicensePlate { get; set; }

    /// <summary>
    /// Gets or sets the derived vehicle status code.
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the preferred image URL used by list thumbnails.
    /// </summary>
    public string ThumbnailUrl { get; set; }

    /// <summary>
    /// Gets or sets the front-side vehicle image URL.
    /// </summary>
    public string FrontImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the side vehicle image URL.
    /// </summary>
    public string SideImageUrl { get; set; }
}
