namespace Haven.Infrastructure.Models.Tenants;

/// <summary>
/// Carries the room, parties, dates, amounts, and lookup data needed to create one rental contract.
/// </summary>
public class TenantContractCreationContextModel
{
    /// <summary>
    /// Gets or sets the scoped room graph.
    /// </summary>
    public Unit Room { get; set; }

    /// <summary>
    /// Gets or sets the landlord party identifier.
    /// </summary>
    public long LandlordPartyId { get; set; }

    /// <summary>
    /// Gets or sets the tenant party identifier.
    /// </summary>
    public long TenantPartyId { get; set; }

    /// <summary>
    /// Gets or sets the contract start date.
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Gets or sets the optional contract end date.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the optional rent amount override.
    /// </summary>
    public decimal? RentAmount { get; set; }

    /// <summary>
    /// Gets or sets the optional deposit amount override.
    /// </summary>
    public decimal? DepositAmount { get; set; }

    /// <summary>
    /// Gets or sets the exact tenant-flow lookup values.
    /// </summary>
    public TenantMasterDataContextModel Lookups { get; set; }
}
