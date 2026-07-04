namespace Haven.Application.Models.Properties.Detail;

/// <summary>
/// Represents a party-scoped property detail request prepared by the application handler.
/// </summary>
public class PropertyDetailRequestModel
{
    /// <summary>
    /// Gets or sets the current party context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the property public identifier.
    /// </summary>
    public Guid PropertyPublicId { get; set; }
}

