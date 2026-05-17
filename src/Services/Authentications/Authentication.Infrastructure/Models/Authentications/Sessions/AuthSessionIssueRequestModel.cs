namespace Authentication.Infrastructure.Models.Authentications.Sessions;

/// <summary>
/// Groups inputs required to issue one Haven authentication session.
/// </summary>
internal sealed class AuthSessionIssueRequestModel
{
    /// <summary>
    /// Gets or sets the authenticated user.
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// Gets or sets the raw refresh token returned to the client.
    /// </summary>
    public string RawRefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the refresh token row that owns the current client session.
    /// </summary>
    public RefreshToken SessionRefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the normalized request device metadata.
    /// </summary>
    public ClientDeviceContextModel DeviceContext { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether token issue should create a new server session id.
    /// </summary>
    public bool RenewSessionPublicId { get; set; }

    /// <summary>
    /// Gets or sets the configured JWT and refresh-token options.
    /// </summary>
    public AuthOptions AuthOptions { get; set; }

    /// <summary>
    /// Gets or sets the token handler used to serialize the JWT.
    /// </summary>
    public JwtSecurityTokenHandler JwtSecurityTokenHandler { get; set; }
}
