namespace Haven.Application.Queries.GetTenants;

/// <summary>
/// Represents a tenant list query.
/// </summary>
public class GetTenantsQuery : IQuery<ResponseDto<PaginationResponse<IReadOnlyList<TenantListItemResponseDto>>>>
{
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
    /// Gets or sets the optional tenant role code.
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
