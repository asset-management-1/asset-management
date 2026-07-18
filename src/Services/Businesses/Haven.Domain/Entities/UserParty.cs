namespace Haven.Domain.Entities;

/// <summary>
/// Represents a permanent relationship between one identity User and one business Party.
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
    /// Gets or sets the internal Party identifier.
    /// </summary>
    public long PartyId { get; set; }

    /// <summary>
    /// Gets or sets the linked business Party.
    /// </summary>
    public Party Party { get; set; }
}
