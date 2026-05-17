namespace Be.Haven.Shared.Dtos.Options.Authentications;

/// <summary>
/// Represents the shared token-validation settings required by Haven authentication handlers.
/// </summary>
public class AuthenticationTokenValidationOptions
{
    /// <summary>
    /// Gets or sets the valid token issuer.
    /// </summary>
    public string Issuer { get; set; }

    /// <summary>
    /// Gets or sets the valid token audiences.
    /// </summary>
    public List<string> Audiences { get; set; } = [];

    /// <summary>
    /// Gets or sets the JWT signature secret value, or the Secret Manager key when secret resolution is enabled.
    /// </summary>
    public string SecretKey { get; set; }

    /// <summary>
    /// Gets or sets the refresh-token lifetime in days, used to cache auth reset state.
    /// </summary>
    public int RefreshTokenDays { get; set; } = AuthConstants.DEFAULT_REFRESH_TOKEN_DAYS;
}
