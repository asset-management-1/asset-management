namespace Authentication.Application.Dtos.Authentications.Passwords;

/// <summary>
/// Represents the forgot-password payload used by the authentication service.
/// </summary>
public class ForgotPasswordRequestDto
{
    public string Email { get; set; }
}
