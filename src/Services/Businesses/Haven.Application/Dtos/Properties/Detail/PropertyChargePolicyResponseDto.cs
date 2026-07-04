namespace Haven.Application.Dtos.Properties.Detail;

/// <summary>
/// Represents a charge policy configured for a property.
/// </summary>
public class PropertyChargePolicyResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe charge policy identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the invoice line/charge type code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the optional vehicle type code for parking policies.
    /// </summary>
    public string VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the optional vehicle type display name for parking policies.
    /// </summary>
    public string VehicleTypeName { get; set; }

    /// <summary>
    /// Gets or sets the charge display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the calculation method code used by the fee setup form.
    /// </summary>
    public string CalculationMethodCode { get; set; }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the policy status code.
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the policy status display name.
    /// </summary>
    public string StatusName { get; set; }
}

