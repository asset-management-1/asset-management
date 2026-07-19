namespace Authentication.Domain.Entities;

/// <summary>
/// Associates one login identity with one authorisation role.
/// </summary>
public class UserRole : BaseEntity
{
    /// <summary>
    /// User ID in the mapping.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Role ID in the mapping.
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// Navigation to mapped role.
    /// </summary>
    public virtual Role Role { get; set; }

    /// <summary>
    /// Navigation to mapped user.
    /// </summary>
    public virtual User User { get; set; }
}
