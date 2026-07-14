namespace Haven.Application.Models.Tenants.QueryParameters;

/// <summary>
/// Dapper parameters for tenant list reads.
/// </summary>
public class TenantListQueryParametersModel
{
    /// <summary>
    /// Gets or sets the current landlord party identifier.
    /// </summary>
    public long CurrentPartyId { get; set; }

    /// <summary>
    /// Gets or sets allowed property relationship codes.
    /// </summary>
    public IReadOnlyCollection<string> RelationshipCodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the optional property public identifier filter.
    /// </summary>
    public Guid? PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the optional room public identifier filter.
    /// </summary>
    public Guid? RoomId { get; set; }

    /// <summary>
    /// Gets or sets the optional search term.
    /// </summary>
    public string Search { get; set; }

    /// <summary>
    /// Gets or sets the optional tenant role code.
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// Gets or sets the optional occupancy status code.
    /// </summary>
    public string OccupancyStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the requested page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the requested page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the row offset.
    /// </summary>
    public int Offset { get; set; }
}
