namespace Haven.Application.Models.Meters.Mutation;

/// <summary>
/// Contains the existing row, effective policy, and prior confirmed baseline for one utility type.
/// </summary>
public sealed class MeterUtilityMutationStateModel
{
    /// <summary>
    /// Gets or sets the electricity or water line-type identifier.
    /// </summary>
    public long LineTypeId { get; init; }

    /// <summary>
    /// Gets or sets the tracked meter row for the selected period when one exists.
    /// </summary>
    public Meter ExistingMeter { get; set; }

    /// <summary>
    /// Gets or sets the effective room override or property common policy.
    /// </summary>
    public MeterPolicyModel EffectivePolicy { get; init; }

    /// <summary>
    /// Gets or sets the latest confirmed value before the selected period.
    /// </summary>
    public decimal? PreviousConfirmedValue { get; init; }
}
