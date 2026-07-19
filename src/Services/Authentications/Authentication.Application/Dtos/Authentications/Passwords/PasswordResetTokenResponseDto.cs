namespace Authentication.Application.Dtos.Authentications.Passwords;

/// <summary>
/// Returns the short-lived authority required to complete a forgot-password reset.
/// </summary>
public class PasswordResetTokenResponseDto
{
    /// <summary>
    /// Gets or sets the opaque one-time password-reset authority.
    /// </summary>
    public string PasswordResetToken { get; set; }

    /// <summary>
    /// Gets or sets the reset-token lifetime in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }
}
