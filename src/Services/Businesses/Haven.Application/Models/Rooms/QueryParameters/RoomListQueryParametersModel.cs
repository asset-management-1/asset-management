namespace Haven.Application.Models.Rooms.QueryParameters;

/// <summary>
/// Parameters used to load the paged landlord room ledger.
/// </summary>
public class RoomListQueryParametersModel
{
    /// <summary>
    /// Gets or sets the current party internal identifier.
    /// </summary>
    public long CurrentPartyId { get; set; }

    /// <summary>
    /// Gets or sets relationship codes that grant property access.
    /// </summary>
    public IReadOnlyCollection<string> RelationshipCodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the optional property public identifier filter.
    /// </summary>
    public Guid? PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the optional property textbox search term.
    /// </summary>
    public string PropertySearch { get; set; }

    /// <summary>
    /// Gets or sets the optional room textbox search term.
    /// </summary>
    public string Search { get; set; }

    /// <summary>
    /// Gets or sets the optional floor number filter.
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the optional room status code filter.
    /// </summary>
    public string RoomStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the optional rental mode code filter.
    /// </summary>
    public string RentalModeCode { get; set; }

    /// <summary>
    /// Gets or sets the optional latest payment status code filter.
    /// </summary>
    public string PaymentStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the number of rows to skip.
    /// </summary>
    public int Offset { get; set; }
}
