namespace Haven.Application.Models.Rooms.Rows;

/// <summary>
/// Row model for one active room tenant.
/// </summary>
public class RoomTenantRowModel
{
    /// <summary>
    /// Gets or sets the room public identifier.
    /// </summary>
    public Guid UnitPublicId { get; set; }

    /// <summary>
    /// Gets or sets the occupancy public identifier exposed as the row id.
    /// </summary>
    public Guid OccupancyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the tenant public identifier.
    /// </summary>
    public Guid TenantPublicId { get; set; }

    /// <summary>
    /// Gets or sets the tenant display name.
    /// </summary>
    public string TenantName { get; set; }

    /// <summary>
    /// Gets or sets the tenant role code derived from the occupancy primary flag.
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// Gets or sets the contract or occupancy start date.
    /// </summary>
    public DateTime? ContractStartDate { get; set; }

    /// <summary>
    /// Gets or sets the contract or occupancy end date.
    /// </summary>
    public DateTime? ContractEndDate { get; set; }

    /// <summary>
    /// Gets or sets the occupancy status code.
    /// </summary>
    public string OccupancyStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the occupancy status display name.
    /// </summary>
    public string OccupancyStatusName { get; set; }

    /// <summary>
    /// Gets or sets the deposit invoice status code when available.
    /// </summary>
    public string DepositStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the deposit invoice status display name when available.
    /// </summary>
    public string DepositStatusName { get; set; }
}
