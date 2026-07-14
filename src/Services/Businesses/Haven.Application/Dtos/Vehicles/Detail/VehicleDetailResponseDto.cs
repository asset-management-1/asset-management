namespace Haven.Application.Dtos.Vehicles.Detail;

/// <summary>
/// Represents the vehicle registration detail used by the standalone vehicle screen.
/// </summary>
public class VehicleDetailResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe vehicle identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the attached room summary.
    /// </summary>
    public VehicleRoomResponseDto Room { get; set; }

    /// <summary>
    /// Gets or sets the primary tenant who receives vehicle charges.
    /// </summary>
    public VehiclePayerResponseDto Payer { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type code.
    /// </summary>
    public string VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type display name.
    /// </summary>
    public string VehicleTypeName { get; set; }

    /// <summary>
    /// Gets or sets the vehicle name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the optional license plate.
    /// </summary>
    public string LicensePlate { get; set; }

    /// <summary>
    /// Gets or sets the optional registration front image URL.
    /// </summary>
    public string RegistrationFrontImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the optional registration side image URL.
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

/// <summary>
/// Represents the compact room context for a vehicle.
/// </summary>
public class VehicleRoomResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the room display name.
    /// </summary>
    public string Name { get; set; }
}

/// <summary>
/// Represents the compact primary tenant payer context for a vehicle.
/// </summary>
public class VehiclePayerResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe tenant party identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant display name.
    /// </summary>
    public string Tenant { get; set; }
}
