namespace Haven.Application.Models.Meters.QueryParameters;

/// <summary>
/// Contains internal scope and date boundaries used by meter read projections.
/// </summary>
public sealed class MeterPeriodQueryParametersModel
{
    /// <summary>
    /// Gets or sets the internal current landlord party identifier.
    /// </summary>
    public long CurrentPartyId { get; set; }

    /// <summary>
    /// Gets or sets the internal property identifier.
    /// </summary>
    public long PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the internal room identifier.
    /// </summary>
    public long UnitId { get; set; }

    /// <summary>
    /// Gets or sets the first date of the selected period.
    /// </summary>
    public DateOnly PeriodFrom { get; set; }

    /// <summary>
    /// Gets or sets the final date of the selected period.
    /// </summary>
    public DateOnly PeriodTo { get; set; }

    /// <summary>
    /// Gets or sets the first date of the preceding period.
    /// </summary>
    public DateOnly PreviousPeriodFrom { get; set; }

    /// <summary>
    /// Gets or sets the final date of the preceding period.
    /// </summary>
    public DateOnly PreviousPeriodTo { get; set; }

    /// <summary>
    /// Gets or sets the first date included by the bounded history query.
    /// </summary>
    public DateOnly HistoryFrom { get; set; }

    /// <summary>
    /// Gets or sets the final date included by the bounded history query.
    /// </summary>
    public DateOnly HistoryTo { get; set; }

    /// <summary>
    /// Gets or sets the active relationship codes that authorize landlord access.
    /// </summary>
    public IReadOnlyCollection<string> RelationshipCodes { get; set; }

    /// <summary>
    /// Gets or sets the canonical status code used to identify confirmed prior periods.
    /// </summary>
    public string ConfirmedMeterStatusCode { get; set; }
}
