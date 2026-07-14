namespace Haven.Domain.Entities;

/// <summary>
/// Represents one electricity or water meter reading for a property or room billing month.
/// </summary>
public class Meter : BaseEntity
{
    public long PropertyId { get; set; }
    public long? UnitId { get; set; }
    public long? ChargePolicyId { get; set; }
    public long LineTypeId { get; set; }
    public DateOnly BillingPeriodFrom { get; set; }
    public DateOnly BillingPeriodTo { get; set; }
    public DateOnly ReadingDate { get; set; }
    public decimal? PreviousReading { get; set; }
    public decimal? CurrentReading { get; set; }
    public decimal? UsageQuantity { get; set; }
    public decimal? UnitPriceSnapshot { get; set; }
    public long? SubmittedByPartyId { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public long? ReviewedByPartyId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public long StatusId { get; set; }
    public Property Property { get; set; }
    public Unit Unit { get; set; }
    public RentalChargePolicy ChargePolicy { get; set; }
}
