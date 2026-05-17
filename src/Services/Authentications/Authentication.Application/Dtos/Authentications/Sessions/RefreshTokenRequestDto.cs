namespace Authentication.Application.Dtos.Authentications.Sessions;

/// <summary>
/// Represents the refresh-token payload used by the authentication service.
/// </summary>
public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; }
}
