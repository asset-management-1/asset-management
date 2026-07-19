namespace Haven.Domain.Entities;

/// <summary>
/// Stores the immutable meter and price snapshot for one utility invoice line.
/// </summary>
public class InvoiceLineUtility : BaseEntity
{
    /// <summary>
    /// Gets or sets the owning invoice-line identifier.
    /// </summary>
    public long InvoiceLineId { get; set; }

    /// <summary>
    /// Gets or sets the utility charge-mode master-data identifier.
    /// </summary>
    public long ChargeModeId { get; set; }

    /// <summary>
    /// Gets or sets the source meter identifier.
    /// </summary>
    public long? MeterId { get; set; }

    /// <summary>
    /// Gets or sets the meter reading date snapshot.
    /// </summary>
    public DateOnly? ReadingDate { get; set; }

    /// <summary>
    /// Gets or sets the previous reading snapshot.
    /// </summary>
    public decimal? PreviousReading { get; set; }

    /// <summary>
    /// Gets or sets the current reading snapshot.
    /// </summary>
    public decimal? CurrentReading { get; set; }

    /// <summary>
    /// Gets or sets the billed usage quantity snapshot.
    /// </summary>
    public decimal? UsageQuantity { get; set; }

    /// <summary>
    /// Gets or sets the billed unit price snapshot.
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the owning invoice-line navigation.
    /// </summary>
    public InvoiceLine InvoiceLine { get; set; }

    /// <summary>
    /// Gets or sets the source meter navigation.
    /// </summary>
    public Meter Meter { get; set; }
}
