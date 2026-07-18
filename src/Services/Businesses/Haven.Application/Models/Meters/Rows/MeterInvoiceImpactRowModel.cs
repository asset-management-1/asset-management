namespace Haven.Application.Models.Meters.Rows;

/// <summary>
/// Projects the invoice state affected by one room meter mutation.
/// </summary>
public sealed class MeterInvoiceImpactRowModel
{
    /// <summary>
    /// Gets or sets the frontend-safe invoice identifier.
    /// </summary>
    public Guid InvoicePublicId { get; set; }

    /// <summary>
    /// Gets or sets the invoice status code.
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the amount already paid against the invoice.
    /// </summary>
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a successful payment is allocated to the invoice.
    /// </summary>
    public bool HasSuccessfulPayment { get; set; }
}
