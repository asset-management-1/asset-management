namespace Haven.Domain.Entities;

/// <summary>
/// Represents one monetary line inside an invoice.
/// </summary>
public class InvoiceLine : BaseEntity
{
    /// <summary>
    /// Gets or sets the owning invoice identifier.
    /// </summary>
    public long InvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the invoice-line type master-data identifier.
    /// </summary>
    public long LineTypeId { get; set; }

    /// <summary>
    /// Gets or sets the source rental charge policy identifier.
    /// </summary>
    public long? ChargePolicyId { get; set; }

    /// <summary>
    /// Gets or sets the billed vehicle identifier when applicable.
    /// </summary>
    public long? PartyVehicleId { get; set; }

    /// <summary>
    /// Gets or sets the user-facing line description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the monetary line amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the optional billing note.
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// Gets or sets the owning invoice navigation.
    /// </summary>
    public Invoice Invoice { get; set; }

    /// <summary>
    /// Gets or sets the utility snapshot when this is a metered line.
    /// </summary>
    public InvoiceLineUtility Utility { get; set; }
}
