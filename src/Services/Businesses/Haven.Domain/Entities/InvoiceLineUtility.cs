namespace Haven.Domain.Entities;

/// <summary>
/// Stores the immutable meter and price snapshot for one utility invoice line.
/// </summary>
public class InvoiceLineUtility : BaseEntity
{
    public long InvoiceLineId { get; set; }
    public long ChargeModeId { get; set; }
    public long? MeterId { get; set; }
    public DateOnly? ReadingDate { get; set; }
    public decimal? PreviousReading { get; set; }
    public decimal? CurrentReading { get; set; }
    public decimal? UsageQuantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public InvoiceLine InvoiceLine { get; set; }
    public Meter Meter { get; set; }
}
