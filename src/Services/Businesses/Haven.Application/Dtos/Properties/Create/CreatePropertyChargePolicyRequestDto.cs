namespace Haven.Application.Dtos.Properties.Create;

/// <summary>
/// Represents one property-level charge policy request.
/// </summary>
public class CreatePropertyChargePolicyRequestDto : IChargePolicyInput
{
    /// <summary>
    /// Gets or sets the invoice line type code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the display charge name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the amount stored in asset.RentalChargePolicies.Amount.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Gets or sets the calculation method code from the UI flow.
    /// </summary>
    public string CalculationMethodCode { get; set; }

    /// <summary>
    /// Gets or sets optional vehicle type code for parking policies.
    /// </summary>
    public string VehicleTypeCode { get; set; }
}

