namespace Haven.Domain.Entities;

/// <summary>
/// Represents one monetary line inside an invoice.
/// </summary>
public class InvoiceLine : BaseEntity
{
    public long InvoiceId { get; set; }
    public long LineTypeId { get; set; }
    public long? ChargePolicyId { get; set; }
    public long? PartyVehicleId { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Note { get; set; }
    public Invoice Invoice { get; set; }
    public InvoiceLineUtility Utility { get; set; }
}
