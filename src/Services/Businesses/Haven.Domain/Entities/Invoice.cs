namespace Haven.Domain.Entities;

/// <summary>
/// Represents one tenant invoice and its replacement lineage.
/// </summary>
public class Invoice : BaseEntity
{
    /// <summary>
    /// Gets or sets the public-facing invoice code.
    /// </summary>
    public string InvoiceCode { get; set; }

    /// <summary>
    /// Gets or sets the optional rental contract identifier.
    /// </summary>
    public long? ContractId { get; set; }

    /// <summary>
    /// Gets or sets the optional subscription identifier.
    /// </summary>
    public long? SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the party that must pay the invoice.
    /// </summary>
    public long BillToPartyId { get; set; }

    /// <summary>
    /// Gets or sets the invoice-type master-data identifier.
    /// </summary>
    public long InvoiceTypeId { get; set; }

    /// <summary>
    /// Gets or sets the inclusive billing-period start.
    /// </summary>
    public DateOnly? BillingPeriodFrom { get; set; }

    /// <summary>
    /// Gets or sets the inclusive billing-period end.
    /// </summary>
    public DateOnly? BillingPeriodTo { get; set; }

    /// <summary>
    /// Gets or sets the invoice due date.
    /// </summary>
    public DateOnly DueDate { get; set; }

    /// <summary>
    /// Gets or sets the total amount across active invoice lines.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the amount already paid.
    /// </summary>
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// Gets or sets the remaining payable balance.
    /// </summary>
    public decimal BalanceAmount { get; set; }

    /// <summary>
    /// Gets or sets the invoice-status master-data identifier.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets or sets the invoice replaced by this invoice.
    /// </summary>
    public long? ReplacesInvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the rental contract navigation.
    /// </summary>
    public Contract Contract { get; set; }

    /// <summary>
    /// Gets or sets the replaced invoice navigation.
    /// </summary>
    public Invoice ReplacesInvoice { get; set; }

    /// <summary>
    /// Gets replacement invoices created from this invoice.
    /// </summary>
    public ICollection<Invoice> ReplacementInvoices { get; } = new List<Invoice>();

    /// <summary>
    /// Gets the monetary lines that compose this invoice.
    /// </summary>
    public ICollection<InvoiceLine> InvoiceLines { get; } = new List<InvoiceLine>();
}
