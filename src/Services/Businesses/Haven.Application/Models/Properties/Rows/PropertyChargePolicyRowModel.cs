namespace Haven.Application.Models.Properties.Rows;

/// <summary>
/// Row model for a property-level charge policy.
/// </summary>
public class PropertyChargePolicyRowModel
{
    /// <summary>
    /// Gets or sets the public policy identifier.
    /// </summary>
    public Guid PolicyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the invoice line/charge type code.
    /// </summary>
    public string ChargeTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the invoice line/charge type display name.
    /// </summary>
    public string ChargeTypeName { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type code for parking policies.
    /// </summary>
    public string VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type display name for parking policies.
    /// </summary>
    public string VehicleTypeName { get; set; }

    /// <summary>
    /// Gets or sets the display charge name.
    /// </summary>
    public string ChargeName { get; set; }

    /// <summary>
    /// Gets or sets whether the charge is usage-based.
    /// </summary>
    public bool IsUsageBased { get; set; }

    /// <summary>
    /// Gets or sets the charge amount.
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

