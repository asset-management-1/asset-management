namespace Haven.Infrastructure.Models.Properties;

/// <summary>
/// Represents one property charge-policy edit with its resolved persistence identifiers.
/// </summary>
public sealed class PropertyChargePolicyMutationModel
{
    /// <summary>
    /// Gets or sets the submitted property charge-policy row.
    /// </summary>
    public UpdatePropertyChargePolicyRequestDto Policy { get; set; }

    /// <summary>
    /// Gets or sets the master-data identifiers resolved for the submitted row.
    /// </summary>
    public RentalChargePolicyLookupModel Lookups { get; set; }
}
