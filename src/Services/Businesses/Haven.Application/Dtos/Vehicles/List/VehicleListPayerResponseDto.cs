namespace Haven.Application.Dtos.Vehicles.List;

/// <summary>
/// Represents the compact payer context for a vehicle list item.
/// </summary>
public sealed class VehicleListPayerResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe tenant party identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the payer display name.
    /// </summary>
    public string Tenant { get; set; }
}
