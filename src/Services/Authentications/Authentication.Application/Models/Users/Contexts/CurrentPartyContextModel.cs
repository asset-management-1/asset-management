namespace Authentication.Application.Models.Users.Contexts;

/// <summary>
/// Represents the minimal party-context data needed to validate current user context.
/// </summary>
public sealed class CurrentPartyContextModel
{
    /// <summary>
    /// Gets or sets the internal party identifier used for owned writes.
    /// </summary>
    public long PartyId { get; set; }

    /// <summary>
    /// Gets or sets the party-type master-data code.
    /// </summary>
    public string PartyTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the party-type master-data display name.
    /// </summary>
    public string PartyTypeName { get; set; }
}
