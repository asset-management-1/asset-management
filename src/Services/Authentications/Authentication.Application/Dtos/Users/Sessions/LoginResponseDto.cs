namespace Authentication.Application.Dtos.Users.Sessions;

/// <summary>
/// Represents the authentication token payload returned by auth endpoints.
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// The issued bearer token string used for authorisation (e.g., JWT or opaque token).
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// Lifetime of the access token in seconds from the time of issuance.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Token type identifier. Typically <c>Bearer</c>.
    /// </summary>
    public string TokenType { get; set; }

    /// <summary>
    /// The token used to obtain a new access token without requiring reauthentication.
    /// </summary>
    public string RefreshToken { get; set; }
}
