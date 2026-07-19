namespace Haven.Application.Models.Properties.QueryParameters;

/// <summary>
/// Parameters used to load the paged property list.
/// </summary>
public class PropertyListQueryParametersModel
{
    /// <summary>
    /// Gets or sets the current party identifier.
    /// </summary>
    public long CurrentPartyId { get; set; }

    /// <summary>
    /// Gets or sets relationship codes that grant property access.
    /// </summary>
    public IReadOnlyCollection<string> RelationshipCodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the optional property search text.
    /// </summary>
    public string Search { get; set; }

    /// <summary>
    /// Gets or sets the optional property-type code.
    /// </summary>
    public string PropertyTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the optional property-status code.
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the optional floor-number filter.
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the optional room-status code.
    /// </summary>
    public string RoomStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the optional payment-status code.
    /// </summary>
    public string PaymentStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the requested page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the row offset for the requested page.
    /// </summary>
    public int Offset { get; set; }
}

