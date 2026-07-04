namespace Haven.Application.Models.Properties.QueryParameters;

/// <summary>
/// Parameters used to load the paged property list.
/// </summary>
public class PropertyListQueryParametersModel
{
    public long CurrentPartyId { get; set; }

    public IReadOnlyCollection<string> RelationshipCodes { get; set; } = [];

    public string Search { get; set; }

    public string PropertyTypeCode { get; set; }

    public string StatusCode { get; set; }

    public int? FloorNumber { get; set; }

    public string RoomStatusCode { get; set; }

    public string PaymentStatusCode { get; set; }

    public int PageSize { get; set; }

    public int Offset { get; set; }
}

