namespace Haven.Domain.Entities;

/// <summary>
/// Represents a rental contract record in leasing.Contracts.
/// </summary>
public class Contract : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique contract code.
    /// </summary>
    public string ContractCode { get; set; }

    /// <summary>
    /// Gets or sets the contract type master-data identifier.
    /// </summary>
    public long ContractTypeId { get; set; }

    /// <summary>
    /// Gets or sets the contract source master-data identifier.
    /// </summary>
    public long ContractSourceId { get; set; }

    /// <summary>
    /// Gets or sets the property identifier when this is a rental contract.
    /// </summary>
    public long? PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the rented unit identifier when this is a rental contract.
    /// </summary>
    public long? UnitId { get; set; }

    /// <summary>
    /// Gets or sets the selected unit package identifier.
    /// </summary>
    public long? UnitPackageId { get; set; }

    /// <summary>
    /// Gets or sets the landlord or primary party identifier.
    /// </summary>
    public long PrimaryPartyId { get; set; }

    /// <summary>
    /// Gets or sets the tenant or secondary party identifier.
    /// </summary>
    public long? SecondaryPartyId { get; set; }

    /// <summary>
    /// Gets or sets the contract effective start date.
    /// </summary>
    public DateOnly EffectiveFrom { get; set; }

    /// <summary>
    /// Gets or sets the contract effective end date.
    /// </summary>
    public DateOnly? EffectiveTo { get; set; }

    /// <summary>
    /// Gets or sets the final rent amount for the contract.
    /// </summary>
    public decimal? RentAmount { get; set; }

    /// <summary>
    /// Gets or sets the final deposit amount for the contract.
    /// </summary>
    public decimal? DepositAmount { get; set; }

    /// <summary>
    /// Gets or sets the contract status master-data identifier.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets or sets the day of month on which recurring invoices are generated.
    /// </summary>
    public int? BillingDayOfMonth { get; set; }

    /// <summary>
    /// Gets or sets the recurring invoice payment due day.
    /// </summary>
    public int? PaymentDueDay { get; set; }

    /// <summary>
    /// Gets the occupancies linked to this contract.
    /// </summary>
    public ICollection<Occupancy> Occupancies { get; } = new List<Occupancy>();

    /// <summary>
    /// Gets the invoices generated for this contract.
    /// </summary>
    public ICollection<Invoice> Invoices { get; } = new List<Invoice>();
}
