namespace Authentication.Application.Dtos.Authentications.Passwords;

/// <summary>
/// Represents the logged-in change-password payload used by the authentication service.
/// </summary>
public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}
