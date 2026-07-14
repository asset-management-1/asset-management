namespace Haven.Application.Models.Meters.Rows;

/// <summary>
/// Projects one flat meter row used to compose room history and period responses.
/// </summary>
public sealed class MeterRowModel
{
    /// <summary>
    /// Gets or sets the frontend-safe property identifier.
    /// </summary>
    public Guid PropertyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the property display name.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid RoomPublicId { get; set; }

    /// <summary>
    /// Gets or sets the room display name.
    /// </summary>
    public string RoomName { get; set; }

    /// <summary>
    /// Gets or sets the frontend-safe meter record identifier.
    /// </summary>
    public Guid? MeterPublicId { get; set; }

    /// <summary>
    /// Gets or sets the internal meter record identifier used to join evidence.
    /// </summary>
    public long? MeterId { get; set; }

    /// <summary>
    /// Gets or sets the electricity or water line type code.
    /// </summary>
    public string LineTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the first date of the represented billing period.
    /// </summary>
    public DateOnly BillingPeriodFrom { get; set; }

    /// <summary>
    /// Gets or sets the date on which the meter value was recorded.
    /// </summary>
    public DateOnly ReadingDate { get; set; }

    /// <summary>
    /// Gets or sets the previous meter value.
    /// </summary>
    public decimal? PreviousReading { get; set; }

    /// <summary>
    /// Gets or sets the current meter value.
    /// </summary>
    public decimal? CurrentReading { get; set; }

    /// <summary>
    /// Gets or sets the calculated consumption quantity.
    /// </summary>
    public decimal? UsageQuantity { get; set; }

    /// <summary>
    /// Gets or sets the confirmed unit price snapshot.
    /// </summary>
    public decimal? UnitPriceSnapshot { get; set; }

    /// <summary>
    /// Gets or sets the price suggested by the effective policy.
    /// </summary>
    public decimal? SuggestedUnitPrice { get; set; }

    /// <summary>
    /// Gets or sets whether the previous value came from an earlier confirmed meter period.
    /// </summary>
    public bool HasPriorConfirmedReading { get; set; }

    /// <summary>
    /// Gets or sets the meter status code.
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the effective calculation method code.
    /// </summary>
    public string CalculationMethodCode { get; set; }

    /// <summary>
    /// Gets or sets the related invoice status code when an invoice exists.
    /// </summary>
    public string InvoiceStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the amount already paid against the related invoice.
    /// </summary>
    public decimal? InvoicePaidAmount { get; set; }

    /// <summary>
    /// Gets or sets whether the related invoice has a successful payment.
    /// </summary>
    public bool HasInvoicePayment { get; set; }
}
