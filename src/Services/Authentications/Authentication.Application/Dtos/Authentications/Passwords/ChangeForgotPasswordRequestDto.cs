namespace Authentication.Application.Dtos.Authentications.Passwords;

/// <summary>
/// Represents the forgot-password change-password payload used by the authentication service.
/// </summary>
public class ChangeForgotPasswordRequestDto
{
    /// <summary>
    /// Gets or sets the normalised email authorised by the consumed reset session.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the validated replacement password.
    /// </summary>
    public string NewPassword { get; set; }
}
