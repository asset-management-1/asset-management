namespace Authentication.Domain.Entities;

public class RefreshToken : BaseEntity
{
    /// <summary>
    /// Associated user ID.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Server-issued public identifier for the current client-instance session.
    /// </summary>
    public Guid? SessionPublicId { get; set; }

    /// <summary>
    /// Hash of refresh token value.
    /// </summary>
    public string TokenHash { get; set; }

    /// <summary>
    /// Previous refresh-token hash kept for near-term reuse detection after in-place rotation.
    /// </summary>
    public string PreviousTokenHash { get; set; }

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
    /// Client-supplied client instance identifier used to keep one client session per app/browser instance.
    /// </summary>
    public string DeviceId { get; set; }

    /// <summary>
    /// Latest client-supplied display name for the app/browser instance.
    /// </summary>
    public string DeviceName { get; set; }

    /// <summary>
    /// Latest client-supplied device type for the app/browser instance.
    /// </summary>
    public string DeviceType { get; set; }

    /// <summary>
    /// Latest user-agent observed for the client session.
    /// </summary>
    public string UserAgent { get; set; }

    /// <summary>
    /// Latest remote IP address observed for the client session.
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    /// UTC timestamp when this client session was last used.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Navigation to associated user.
    /// </summary>
    public virtual User User { get; set; }
}
