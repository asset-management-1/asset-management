namespace Haven.Domain.Entities;

/// <summary>
/// Represents one tenant invoice and its replacement lineage.
/// </summary>
public class Invoice : BaseEntity
{
    public string InvoiceCode { get; set; }
    public long? ContractId { get; set; }
    public long? SubscriptionId { get; set; }
    public long BillToPartyId { get; set; }
    public long InvoiceTypeId { get; set; }
    public DateOnly? BillingPeriodFrom { get; set; }
    public DateOnly? BillingPeriodTo { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public long StatusId { get; set; }
    public long? ReplacesInvoiceId { get; set; }
    public Contract Contract { get; set; }
    public Invoice ReplacesInvoice { get; set; }
    public ICollection<Invoice> ReplacementInvoices { get; } = new List<Invoice>();
    public ICollection<InvoiceLine> InvoiceLines { get; } = new List<InvoiceLine>();
}
