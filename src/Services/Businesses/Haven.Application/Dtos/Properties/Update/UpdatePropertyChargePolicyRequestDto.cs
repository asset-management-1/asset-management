namespace Haven.Application.Dtos.Properties.Update;

/// <summary>
/// Represents one property-level charge policy submitted by the edit form.
/// </summary>
public class UpdatePropertyChargePolicyRequestDto : IChargePolicyInput
{
    /// <summary>
    /// Gets or sets the frontend-safe policy identifier for existing policies.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the invoice line type code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the display charge name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the charge amount.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Gets or sets the calculation method code from the edit form.
    /// </summary>
    public string CalculationMethodCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type code for parking policies.
    /// </summary>
    public string VehicleTypeCode { get; set; }
}
