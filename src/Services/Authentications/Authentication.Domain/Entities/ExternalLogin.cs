namespace Authentication.Domain.Entities;

/// <summary>
/// Links a Haven user account to one identity issued by an external authentication provider.
/// </summary>
public class ExternalLogin : BaseEntity
{
    /// <summary>
    /// Associated user ID.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// External provider name (e.g. Google, Facebook).
    /// </summary>
    public string LoginProvider { get; set; }

    /// <summary>
    /// Unique user key from the external provider.
    /// </summary>
    public string ProviderKey { get; set; }

    /// <summary>
    /// Navigation to the associated user.
    /// </summary>
    public virtual User User { get; set; }
}
