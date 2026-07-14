namespace Haven.Application.Dtos.Rooms.Detail;

/// <summary>
/// Represents one effective room charge policy.
/// </summary>
public class RoomChargePolicyResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe charge policy identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the invoice line type code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the charge display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the charge amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the calculation method code.
    /// </summary>
    public string CalculationMethodCode { get; set; }

    /// <summary>
    /// Gets or sets the optional vehicle type code for parking policies.
    /// </summary>
    public string VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the optional vehicle type display name for parking policies.
    /// </summary>
    public string VehicleTypeName { get; set; }

    /// <summary>
    /// Gets or sets the policy status code.
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the policy status display name.
    /// </summary>
    public string StatusName { get; set; }
}
