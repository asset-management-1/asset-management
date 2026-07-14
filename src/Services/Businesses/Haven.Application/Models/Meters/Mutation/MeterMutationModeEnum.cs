namespace Haven.Application.Models.Meters.Mutation;

/// <summary>
/// Identifies the existence rule applied by the shared room meter mutation workflow.
/// </summary>
public enum MeterMutationModeEnum
{
    /// <summary>
    /// Requires the selected room meter period not to exist.
    /// </summary>
    Create = 1,

    /// <summary>
    /// Requires the selected room meter period to exist.
    /// </summary>
    Update = 2
}
