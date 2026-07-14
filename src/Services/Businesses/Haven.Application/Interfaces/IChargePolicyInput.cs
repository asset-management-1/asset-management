namespace Haven.Application.Interfaces;

/// <summary>
/// Describes a charge-policy payload that can be mapped into rental charge policy fields.
/// </summary>
public interface IChargePolicyInput
{
    /// <summary>
    /// Gets the invoice line type code.
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Gets the display charge name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the charge amount.
    /// </summary>
    decimal? Amount { get; }

    /// <summary>
    /// Gets the calculation method code.
    /// </summary>
    string CalculationMethodCode { get; }

    /// <summary>
    /// Gets the vehicle type code for parking policies.
    /// </summary>
    string VehicleTypeCode { get; }
}
