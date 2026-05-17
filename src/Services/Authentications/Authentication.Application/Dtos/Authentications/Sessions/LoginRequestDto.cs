namespace Authentication.Application.Dtos.Authentications.Sessions;

/// <summary>
/// Represents the local-credential login payload used by the authentication service.
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Gets or sets the username submitted by the client.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the plaintext password submitted by the client.
    /// </summary>
    public string Password { get; set; }
}
