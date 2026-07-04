namespace Haven.Application.Models.Properties.QueryParameters;

/// <summary>
/// Parameters used to load floor and room child rows for selected properties.
/// </summary>
public class PropertyChildRowsQueryParametersModel
{
    /// <summary>
    /// Gets or sets the selected property public identifiers.
    /// </summary>
    public IReadOnlyCollection<Guid> PropertyPublicIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the optional textbox search term.
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
    /// Gets or sets the optional latest payment status code filter.
    /// </summary>
    public string PaymentStatusCode { get; set; }
}

