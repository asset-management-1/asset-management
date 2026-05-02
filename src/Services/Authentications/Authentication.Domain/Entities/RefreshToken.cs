namespace Authentication.Domain.Entities;

public partial class RefreshToken : BaseEntity
{
    /// <summary>
    /// Associated user ID.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Hash of refresh token value.
    /// </summary>
    public string TokenHash { get; set; }

    /// <summary>
    /// JWT ID tied to this refresh token.
    /// </summary>
    public string JwtId { get; set; }

    /// <summary>
    /// UTC expiration time.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// UTC revoke time if token is revoked.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Hash of the token that replaced this one.
    /// </summary>
    public string ReplacedByTokenHash { get; set; }

    /// <summary>
    /// Navigation to associated user.
    /// </summary>
    public virtual User User { get; set; }
}
