namespace Haven.Domain.Entities;

/// <summary>
/// Represents an authentication account in identity.Users.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Gets or sets the selected current party identifier.
    /// </summary>
    public long? CurrentPartyId { get; set; }

    /// <summary>
    /// Gets or sets the login username.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the login email.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the login phone number.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the user full name.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Gets or sets the avatar URL.
    /// </summary>
    public string AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the user status master-data identifier.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets the business parties linked to this user account.
    /// </summary>
    public ICollection<UserParty> UserParties { get; } = new List<UserParty>();
}
