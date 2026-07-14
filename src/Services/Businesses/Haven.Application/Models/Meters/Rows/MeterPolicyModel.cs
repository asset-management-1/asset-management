namespace Haven.Application.Models.Meters.Rows;

/// <summary>
/// Represents the persistence values needed to calculate one meter charge.
/// </summary>
public sealed class MeterPolicyModel
{
    /// <summary>
    /// Gets or sets the internal charge-policy identifier persisted on the meter record.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the effective default price for one consumed utility unit.
    /// </summary>
    public decimal Amount { get; set; }
}
