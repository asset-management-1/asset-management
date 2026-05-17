namespace Authentication.Domain.Entities;

public class User : BaseEntity
{
    /// <summary>
    /// Active party context currently selected for the user.
    /// </summary>
    public long? CurrentPartyId { get; set; }

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
    /// User date of birth.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// User gender master-data value id.
    /// </summary>
    public long? GenderId { get; set; }

    /// <summary>
    /// Status master data value ID.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// UTC timestamp of last successful login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// UTC timestamp after which newly issued access tokens remain valid.
    /// </summary>
    public DateTime? AuthResetAt { get; set; }

    /// <summary>
    /// Navigation collection of external logins.
    /// </summary>
    public virtual ICollection<ExternalLogin> ExternalLogins { get; set; } = new List<ExternalLogin>();

    /// <summary>
    /// Navigation to the active party context currently selected by the user.
    /// </summary>
    public virtual Party CurrentParty { get; set; }

    /// <summary>
    /// Navigation collection of refresh tokens.
    /// </summary>
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>
    /// Navigation to gender master data value.
    /// </summary>
    public virtual MasterDataValue Gender { get; set; }

    /// <summary>
    /// Navigation to status master data value.
    /// </summary>
    public virtual MasterDataValue Status { get; set; }

    /// <summary>
    /// Navigation collection of user-role mappings.
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// Navigation collection of user-party mappings.
    /// </summary>
    public virtual ICollection<UserParty> UserParties { get; set; } = new List<UserParty>();
}
