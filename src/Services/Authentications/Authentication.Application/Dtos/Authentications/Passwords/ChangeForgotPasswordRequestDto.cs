namespace Authentication.Application.Dtos.Authentications.Passwords;

/// <summary>
/// Represents the forgot-password change-password payload used by the authentication service.
/// </summary>
public class ChangeForgotPasswordRequestDto
{
    public string Email { get; set; }
    public string NewPassword { get; set; }
}
