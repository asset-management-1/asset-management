namespace Authentication.Application.Dtos.Users.Vehicles;

/// <summary>
/// Represents one tenant profile vehicle returned to account/profile clients.
/// </summary>
public class UserVehicleResponseDto
{
    /// <summary>
    /// Gets or sets the public vehicle identifier used by API clients.
    /// </summary>
    public Guid PublicId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type.
    /// </summary>
    public VehicleTypeEnum VehicleType { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type display name.
    /// </summary>
    public string VehicleTypeDisplayName { get; set; }

    /// <summary>
    /// Gets or sets the vehicle display name.
    /// </summary>
    public string VehicleName { get; set; }

    /// <summary>
    /// Gets or sets the optional vehicle license plate value.
    /// </summary>
    public string LicensePlate { get; set; }

    /// <summary>
    /// Gets or sets the derived vehicle status returned for active profile vehicles.
    /// </summary>
    public UserVehicleStatusEnum Status { get; set; }

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
