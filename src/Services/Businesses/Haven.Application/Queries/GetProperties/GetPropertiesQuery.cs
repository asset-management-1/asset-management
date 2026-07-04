namespace Haven.Application.Queries.GetProperties;

/// <summary>
/// Represents a landlord-scoped query for the property/building list screen.
/// </summary>
public class GetPropertiesQuery : IQuery<ResponseDto<PaginationResponse<IReadOnlyList<PropertyListItemResponseDto>>>>
{
    /// <summary>
    /// Gets or sets the optional search term for property or room.
    /// </summary>
    public string Search { get; set; }

    /// <summary>
    /// Gets or sets the optional property type code filter.
    /// </summary>
    public string PropertyTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the optional property status code filter.
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the optional floor number filter.
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the optional room/unit status code filter.
    /// </summary>
    public string RoomStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the optional latest invoice/payment status code filter.
    /// </summary>
    public string PaymentStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the requested page number.
    /// </summary>
    public int PageNumber { get; set; } = DEFAULT_PAGE_NUMBER;

    /// <summary>
    /// Gets or sets the requested page size.
    /// </summary>
    public int PageSize { get; set; } = DEFAULT_PROPERTY_PAGE_SIZE;

}
