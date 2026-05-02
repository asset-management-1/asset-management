namespace Authentication.Domain.Entities;

public partial class User : BaseEntity
{
    /// <summary>
    /// Linked party ID from Party service.
    /// </summary>
    public long? PartyId { get; set; }

    /// <summary>
    /// Unique username for login.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// User email address.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// User phone number.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Password hash for local authentication.
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    /// Whether email has been verified.
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Whether phone number has been verified.
    /// </summary>
    public bool PhoneConfirmed { get; set; }

    /// <summary>
    /// User full name.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// URL to user avatar image.
    /// </summary>
    public string AvatarUrl { get; set; }

    /// <summary>
    /// Status master data value ID.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// UTC timestamp of last successful login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Navigation collection of external logins.
    /// </summary>
    public virtual ICollection<ExternalLogin> ExternalLogins { get; set; } = new List<ExternalLogin>();

    /// <summary>
    /// Navigation collection of refresh tokens.
    /// </summary>
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>
    /// Navigation to status master data value.
    /// </summary>
    public virtual MasterDataValue Status { get; set; }

    /// <summary>
    /// Navigation collection of user-role mappings.
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
