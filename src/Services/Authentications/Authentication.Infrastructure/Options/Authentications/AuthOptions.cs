namespace Authentication.Infrastructure.Options.Authentications;

/// <summary>
/// Represents JWT authentication settings for the Haven authentication module.
/// </summary>
public class AuthOptions
{
    /// <summary>
    /// JWT issuer value.
    /// </summary>
    [Required]
    public string Issuer { get; set; }

    /// <summary>
    /// Allowed JWT audiences.
    /// </summary>
    [Required, MinLength(1)]
    public List<string> Audiences { get; set; } = [];

    /// <summary>
    /// Resolved JWT signature secret used to sign and validate tokens.
    /// </summary>
    [Required, MinLength(32)]
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
