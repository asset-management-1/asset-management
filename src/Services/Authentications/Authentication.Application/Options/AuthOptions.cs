namespace Authentication.Application.Options;

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
    /// Secret key used for signing JWT tokens.
    /// </summary>
    public string SecretKey { get; set; }

    /// <summary>
    /// Access token lifetime in minutes.
    /// </summary>
    public int AccessTokenMinutes { get; set; } = 30;

    /// <summary>
    /// Refresh token lifetime in days.
    /// </summary>
    public int RefreshTokenDays { get; set; } = 30;
}
