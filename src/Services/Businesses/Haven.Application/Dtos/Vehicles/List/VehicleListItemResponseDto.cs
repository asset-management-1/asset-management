namespace Haven.Application.Dtos.Vehicles.List;

/// <summary>
/// Represents one vehicle row in the landlord management list.
/// </summary>
public sealed class VehicleListItemResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe vehicle identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the primary tenant payer context.
    /// </summary>
    public VehicleListPayerResponseDto Payer { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type code.
    /// </summary>
    public string VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type display name.
    /// </summary>
    public string VehicleTypeName { get; set; }

    /// <summary>
    /// Gets or sets the vehicle display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the optional license plate.
    /// </summary>
    public string LicensePlate { get; set; }
}
