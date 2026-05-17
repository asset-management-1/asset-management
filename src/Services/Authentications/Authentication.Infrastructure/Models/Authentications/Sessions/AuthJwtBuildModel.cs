namespace Authentication.Infrastructure.Models.Authentications.Sessions;

/// <summary>
/// Groups inputs required to build one Haven JWT.
/// </summary>
internal sealed class AuthJwtBuildModel
{
    /// <summary>
    /// Gets or sets the authenticated user.
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// Gets or sets the refresh token row that owns the current client session.
    /// </summary>
    public RefreshToken SessionRefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the configured JWT options.
    /// </summary>
    public AuthOptions AuthOptions { get; set; }

    /// <summary>
    /// Gets or sets the generated JWT identifier.
    /// </summary>
    public string JwtId { get; set; }

    /// <summary>
    /// Gets or sets the UTC issue timestamp.
    /// </summary>
    public DateTime IssuedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC expiry timestamp.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
