namespace Haven.Application.Models.Meters.Mutation;

/// <summary>
/// Defines the exact scope, period, and master-data identifiers required to prepare a meter mutation.
/// </summary>
public sealed class MeterMutationStateRequestModel
{
    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid RoomPublicId { get; init; }

    /// <summary>
    /// Gets or sets the current landlord party identifier.
    /// </summary>
    public long CurrentPartyId { get; init; }

    /// <summary>
    /// Gets or sets the active relationship codes allowed to manage the property.
    /// </summary>
    public IReadOnlyCollection<string> RelationshipCodes { get; init; }

    /// <summary>
    /// Gets or sets the first date of the selected meter period.
    /// </summary>
    public DateOnly PeriodFrom { get; init; }

    /// <summary>
    /// Gets or sets the last date of the selected meter period.
    /// </summary>
    public DateOnly PeriodTo { get; init; }

    /// <summary>
    /// Gets or sets the confirmed meter status identifier used for baseline selection.
    /// </summary>
    public long ConfirmedStatusId { get; init; }

    /// <summary>
    /// Gets or sets the electricity line-type identifier.
    /// </summary>
    public long ElectricLineTypeId { get; init; }

    /// <summary>
    /// Gets or sets the water line-type identifier.
    /// </summary>
    public long WaterLineTypeId { get; init; }
}
