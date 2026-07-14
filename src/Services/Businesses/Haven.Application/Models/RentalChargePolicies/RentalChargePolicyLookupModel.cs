namespace Haven.Application.Models.RentalChargePolicies;

/// <summary>
/// Carries resolved master-data identifiers used while applying charge policy fields.
/// </summary>
public class RentalChargePolicyLookupModel
{
    /// <summary>
    /// Gets or sets the invoice line type identifier.
    /// </summary>
    public long? LineTypeId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type identifier for parking policies.
    /// </summary>
    public long? VehicleTypeId { get; set; }

    /// <summary>
    /// Gets or sets the active status identifier for the policy row.
    /// </summary>
    public long? StatusId { get; set; }
}
