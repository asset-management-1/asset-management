namespace Haven.Application.Models.Tenants.Create;

/// <summary>
/// Represents a request to add a tenant or member to a room.
/// </summary>
public class TenantCreateRequestModel
{
    /// <summary>
    /// Gets or sets the current landlord party context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the room public identifier.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Gets or sets the requested role code.
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// Gets or sets an existing tenant party public identifier.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets an existing account public identifier.
    /// </summary>
    public Guid? TenantAccountId { get; set; }

    /// <summary>
    /// Gets or sets an inline tenant profile for no-account tenants.
    /// </summary>
    public TenantProfileModel TenantProfile { get; set; }

    /// <summary>
    /// Gets or sets the contract or occupancy start date.
    /// </summary>
    public DateTime? ContractStartDate { get; set; }

    /// <summary>
    /// Gets or sets the contract end date.
    /// </summary>
    public DateTime? ContractEndDate { get; set; }

    /// <summary>
    /// Gets or sets the contract rent amount.
    /// </summary>
    public decimal? ContractRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the contract deposit amount.
    /// </summary>
    public decimal? DepositAmount { get; set; }

}
