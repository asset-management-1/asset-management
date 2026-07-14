namespace Haven.Application.Models.Tenants.Rows;

/// <summary>
/// Row model for tenant list and detail reads.
/// </summary>
public class TenantRowModel
{
    /// <summary>
    /// Gets or sets the occupancy public identifier.
    /// </summary>
    public Guid OccupancyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the tenant party public identifier.
    /// </summary>
    public Guid TenantPublicId { get; set; }

    /// <summary>
    /// Gets or sets the tenant display name.
    /// </summary>
    public string TenantName { get; set; }

    /// <summary>
    /// Gets or sets the tenant phone number.
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// Gets or sets the tenant email.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the tenant role code.
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// Gets or sets the property public identifier.
    /// </summary>
    public Guid PropertyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the property display name.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the room public identifier.
    /// </summary>
    public Guid RoomPublicId { get; set; }

    /// <summary>
    /// Gets or sets the room code.
    /// </summary>
    public string RoomCode { get; set; }

    /// <summary>
    /// Gets or sets the room display name.
    /// </summary>
    public string RoomName { get; set; }

    /// <summary>
    /// Gets or sets the contract or occupancy start date.
    /// </summary>
    public DateTime? ContractStartDate { get; set; }

    /// <summary>
    /// Gets or sets the contract or occupancy end date.
    /// </summary>
    public DateTime? ContractEndDate { get; set; }

    /// <summary>
    /// Gets or sets the contract rent amount.
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

    /// <summary>
    /// Gets or sets the total count for paged reads.
    /// </summary>
    public int TotalCount { get; set; }
}
