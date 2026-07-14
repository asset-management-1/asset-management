namespace Haven.Domain.Entities;

/// <summary>
/// Represents a link between an identity user account and a business party.
/// </summary>
public class UserParty : BaseEntity
{
    /// <summary>
    /// Gets or sets the internal user identifier.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Gets or sets the linked user account.
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// Gets or sets the internal party identifier.
    /// </summary>
    public long PartyId { get; set; }

    /// <summary>
    /// Gets or sets the linked business party.
    /// </summary>
    public Party Party { get; set; }
}
