namespace Authentication.Infrastructure.Options.Authentications;

/// <summary>
/// Represents JWT authentication settings for the Haven authentication module.
/// </summary>
public class AuthOptions
{
    /// <summary>
    /// JWT issuer value.
    /// </summary>
    public string Issuer { get; set; }

    /// <summary>
    /// Allowed JWT audiences.
    /// </summary>
    public List<string> Audiences { get; set; } = [];

    /// <summary>
    /// JWT signature secret value, or the Secret Manager key when secret resolution is enabled.
    /// </summary>
    public string SecretKey { get; set; }

    /// <summary>
    /// Access token lifetime in minutes.
    /// </summary>
    public int AccessTokenMinutes { get; set; } = DEFAULT_ACCESS_TOKEN_MINUTES;

    /// <summary>
    /// Refresh token lifetime in days.
    /// </summary>
    public int RefreshTokenDays { get; set; } = DEFAULT_REFRESH_TOKEN_DAYS;
}
