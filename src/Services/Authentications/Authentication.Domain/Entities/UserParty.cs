namespace Authentication.Domain.Entities;

/// <summary>
/// Links one login account to one business party context.
/// </summary>
public class UserParty : BaseEntity
{
    /// <summary>
    /// Internal user identifier.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Internal party identifier.
    /// </summary>
    public long PartyId { get; set; }

    /// <summary>
    /// Navigation to the linked user account.
    /// </summary>
    public virtual User User { get; set; }

    /// <summary>
    /// Navigation to the linked business party.
    /// </summary>
    public virtual Party Party { get; set; }
}
