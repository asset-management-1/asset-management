namespace Authentication.Infrastructure.Models.Authentications.Sessions;

/// <summary>
/// Groups inputs required to build one login response payload.
/// </summary>
internal sealed class AuthLoginResponseBuildModel
{
    /// <summary>
    /// Gets or sets the signed JWT.
    /// </summary>
    public JwtSecurityToken Jwt { get; set; }

    /// <summary>
    /// Gets or sets the raw refresh token returned to the client.
    /// </summary>
    public string RawRefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the configured JWT options.
    /// </summary>
    public AuthOptions AuthOptions { get; set; }

    /// <summary>
    /// Gets or sets the token handler used to serialize the JWT.
    /// </summary>
    public JwtSecurityTokenHandler JwtSecurityTokenHandler { get; set; }
}
