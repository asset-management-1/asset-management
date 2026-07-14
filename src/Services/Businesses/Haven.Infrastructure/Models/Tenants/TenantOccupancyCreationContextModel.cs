namespace Haven.Infrastructure.Models.Tenants;

/// <summary>
/// Carries the room, party, role, contract, and lookup data needed to create one tenant occupancy.
/// </summary>
public class TenantOccupancyCreationContextModel
{
    /// <summary>
    /// Gets or sets the scoped room graph.
    /// </summary>
    public Unit Room { get; set; }

    /// <summary>
    /// Gets or sets the landlord party context.
    /// </summary>
    public CurrentPartyContextModel LandlordParty { get; set; }

    /// <summary>
    /// Gets or sets the tenant party.
    /// </summary>
    public Party TenantParty { get; set; }

    /// <summary>
    /// Gets or sets the requested tenant role code.
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// Gets or sets the optional contract start date.
    /// </summary>
    public DateTime? ContractStartDate { get; set; }

    /// <summary>
    /// Gets or sets the optional contract end date.
    /// </summary>
    public DateTime? ContractEndDate { get; set; }

    /// <summary>
    /// Gets or sets the optional contract rent amount.
    /// </summary>
    public decimal? ContractRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the optional deposit amount.
    /// </summary>
    public decimal? DepositAmount { get; set; }

    /// <summary>
    /// Gets or sets the exact tenant-flow lookup values.
    /// </summary>
    public TenantMasterDataContextModel Lookups { get; set; }
}
