namespace Haven.Application.Models.Parties;

/// <summary>
/// Represents the authenticated user's current party context.
/// </summary>
public class CurrentPartyContextModel
{
    /// <summary>
    /// Gets or sets the internal party identifier.
    /// </summary>
    public long PartyId { get; set; }

    /// <summary>
    /// Gets or sets the party public identifier.
    /// </summary>
    public Guid PartyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the party display name.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the party type code.
    /// </summary>
    public string PartyTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the party status code.
    /// </summary>
    public string StatusCode { get; set; }
}

