namespace Haven.Application.Dtos.Tenants.List;

/// <summary>
/// Represents one tenant occupancy row in the landlord tenant ledger.
/// </summary>
public class TenantListItemResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe occupancy identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the frontend-safe tenant party identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the tenant display name.
    /// </summary>
    public string Tenant { get; set; }

    /// <summary>
    /// Gets or sets the tenant phone number.
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// Gets or sets the tenant role code in the room.
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// Gets or sets the compact property information.
    /// </summary>
    public TenantPropertySummaryResponseDto Property { get; set; }

    /// <summary>
    /// Gets or sets the compact room information.
    /// </summary>
    public TenantRoomSummaryResponseDto Room { get; set; }

    /// <summary>
    /// Gets or sets the contract or occupancy start date.
    /// </summary>
    public DateTime? ContractStartDate { get; set; }

    /// <summary>
    /// Gets or sets the contract or occupancy end date.
    /// </summary>
    public DateTime? ContractEndDate { get; set; }

    /// <summary>
    /// Gets or sets the contract rent amount when the tenant signs a contract.
    /// </summary>
    public decimal? ContractRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the deposit status code when available.
    /// </summary>
    public string DepositStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the deposit status display name when available.
    /// </summary>
    public string DepositStatusName { get; set; }

    /// <summary>
    /// Gets or sets the occupancy status code.
    /// </summary>
    public string OccupancyStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the occupancy status display name.
    /// </summary>
    public string OccupancyStatusName { get; set; }
}
