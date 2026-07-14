namespace Haven.Application.Models.Properties.Delete;

/// <summary>
/// Represents a party-scoped property delete request.
/// </summary>
public class PropertyDeleteRequestModel
{
    /// <summary>
    /// Gets or sets the property public identifier from the route.
    /// </summary>
    public Guid PropertyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the current landlord party context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }
}
