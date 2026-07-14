namespace Haven.Application.Models.Properties.Rows;

/// <summary>
/// Row model for one active tenant displayed inside a room card.
/// </summary>
public class PropertyRoomTenantRowModel
{
    /// <summary>
    /// Gets or sets the parent property public identifier.
    /// </summary>
    public Guid PropertyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the public unit identifier.
    /// </summary>
    public Guid UnitPublicId { get; set; }

    /// <summary>
    /// Gets or sets the occupancy public identifier exposed as the row id.
    /// </summary>
    public Guid OccupancyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the tenant public identifier exposed to the API response.
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
    /// Gets or sets the tenant contract start date.
    /// </summary>
    public DateTime? ContractStartDate { get; set; }

    /// <summary>
    /// Gets or sets the tenant contract end date.
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
