namespace Haven.Application.Models.Tenants.List;

/// <summary>
/// Represents a tenant list request scoped to the current landlord party.
/// </summary>
public class TenantListRequestModel
{
    /// <summary>
    /// Gets or sets the current party context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the optional property filter.
    /// </summary>
    public Guid? PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the optional room filter.
    /// </summary>
    public Guid? RoomId { get; set; }

    /// <summary>
    /// Gets or sets the optional tenant search term.
    /// </summary>
    public string Search { get; set; }

    /// <summary>
    /// Gets or sets the optional role code.
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// Gets or sets the optional occupancy status code.
    /// </summary>
    public string OccupancyStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the page number.
    /// </summary>
    public int PageNumber { get; set; } = DEFAULT_PAGE_NUMBER;

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; } = DEFAULT_ROOM_PAGE_SIZE;
}
