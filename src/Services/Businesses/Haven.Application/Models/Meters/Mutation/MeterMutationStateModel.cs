namespace Haven.Application.Models.Meters.Mutation;

/// <summary>
/// Contains the landlord-scoped and transaction-protected state required to mutate one meter period.
/// </summary>
public sealed class MeterMutationStateModel
{
    /// <summary>
    /// Gets or sets the locked room and property scope.
    /// </summary>
    public MeterScopeModel Scope { get; init; }

    /// <summary>
    /// Gets or sets the electricity mutation state.
    /// </summary>
    public MeterUtilityMutationStateModel Electric { get; init; }

    /// <summary>
    /// Gets or sets the water mutation state.
    /// </summary>
    public MeterUtilityMutationStateModel Water { get; init; }

    /// <summary>
    /// Gets whether any active meter row already exists for the selected period.
    /// </summary>
    public bool HasExistingPeriod => Electric.ExistingMeter is not null || Water.ExistingMeter is not null;

    /// <summary>
    /// Returns the existing and newly attached tracked meter rows for downstream evidence and invoice phases.
    /// </summary>
    /// <returns>The current tracked meter rows.</returns>
    public IReadOnlyList<Meter> GetMeters()
    {
        return new[] { Electric.ExistingMeter, Water.ExistingMeter }
            .Where(meter => meter is not null)
            .ToList();
    }
}
