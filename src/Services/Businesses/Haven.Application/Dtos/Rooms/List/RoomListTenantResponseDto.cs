namespace Haven.Application.Dtos.Rooms.List;

/// <summary>
/// Represents one active tenant displayed inside a room card.
/// </summary>
public class RoomListTenantResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe tenant identifier.
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
}
