namespace Haven.Application.Dtos.Properties.List;

/// <summary>
/// Represents one active tenant row shown inside a room card.
/// </summary>
public class PropertyListRoomTenantResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe tenant identifier used by the FE to open tenant-related actions.
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
    /// Gets or sets the tenant role in the room.
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// Gets or sets the tenant contract start date.
    /// </summary>
    public DateTime? ContractStartDate { get; set; }

    /// <summary>
    /// Gets or sets the tenant contract end date.
    /// </summary>
    public DateTime? ContractEndDate { get; set; }

    /// <summary>
    /// Gets or sets the tenant occupancy status code.
    /// </summary>
    public string OccupancyStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the tenant occupancy status display name.
    /// </summary>
    public string OccupancyStatusName { get; set; }
}
