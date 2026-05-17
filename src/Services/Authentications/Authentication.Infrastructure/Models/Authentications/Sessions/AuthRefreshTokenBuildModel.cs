namespace Authentication.Infrastructure.Models.Authentications.Sessions;

/// <summary>
/// Groups inputs required to build one persisted refresh-token row.
/// </summary>
internal sealed class AuthRefreshTokenBuildModel
{
    /// <summary>
    /// Gets or sets the refresh token row that owns the current client session.
    /// </summary>
    public RefreshToken SessionRefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the internal user identifier that owns the client session.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Gets or sets the normalized request device metadata.
    /// </summary>
    public ClientDeviceContextModel DeviceContext { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether token issue should create a new server session id.
    /// </summary>
    public bool RenewSessionPublicId { get; set; }

    /// <summary>
    /// Gets or sets the raw refresh token returned to the client.
    /// </summary>
    public string RawRefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the generated JWT identifier.
    /// </summary>
    public string JwtId { get; set; }

    /// <summary>
    /// Gets or sets the UTC issue timestamp.
    /// </summary>
    public DateTime IssuedAt { get; set; }

    /// <summary>
    /// Gets or sets the configured refresh-token options.
    /// </summary>
    public AuthOptions AuthOptions { get; set; }
}
