namespace Haven.Domain.Entities;

/// <summary>
/// Represents one electricity or water meter reading for a property or room billing month.
/// </summary>
public class Meter : BaseEntity
{
    /// <summary>
    /// Gets or sets the owning property identifier.
    /// </summary>
    public long PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the owning room identifier.
    /// </summary>
    public long? UnitId { get; set; }

    /// <summary>
    /// Gets or sets the effective charge policy identifier.
    /// </summary>
    public long? ChargePolicyId { get; set; }

    /// <summary>
    /// Gets or sets the electricity or water line-type identifier.
    /// </summary>
    public long LineTypeId { get; set; }

    /// <summary>
    /// Gets or sets the billing-period start.
    /// </summary>
    public DateOnly BillingPeriodFrom { get; set; }

    /// <summary>
    /// Gets or sets the billing-period end.
    /// </summary>
    public DateOnly BillingPeriodTo { get; set; }

    /// <summary>
    /// Gets or sets the meter reading date.
    /// </summary>
    public DateOnly ReadingDate { get; set; }

    /// <summary>
    /// Gets or sets the confirmed previous reading.
    /// </summary>
    public decimal? PreviousReading { get; set; }

    /// <summary>
    /// Gets or sets the submitted current reading.
    /// </summary>
    public decimal? CurrentReading { get; set; }

    /// <summary>
    /// Gets or sets the calculated usage quantity.
    /// </summary>
    public decimal? UsageQuantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price captured for billing.
    /// </summary>
    public decimal? UnitPriceSnapshot { get; set; }

    /// <summary>
    /// Gets or sets the party that submitted the reading.
    /// </summary>
    public long? SubmittedByPartyId { get; set; }

    /// <summary>
    /// Gets or sets the UTC submission time.
    /// </summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// Gets or sets the party that reviewed the reading.
    /// </summary>
    public long? ReviewedByPartyId { get; set; }

    /// <summary>
    /// Gets or sets the UTC review time.
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Gets or sets the meter-status master-data identifier.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets or sets the owning property navigation.
    /// </summary>
    public Property Property { get; set; }

    /// <summary>
    /// Gets or sets the owning room navigation.
    /// </summary>
    public Unit Unit { get; set; }

    /// <summary>
    /// Gets or sets the effective charge policy navigation.
    /// </summary>
    public RentalChargePolicy ChargePolicy { get; set; }
}
