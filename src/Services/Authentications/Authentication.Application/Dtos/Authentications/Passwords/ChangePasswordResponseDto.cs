namespace Authentication.Application.Dtos.Authentications.Passwords;

/// <summary>
/// Returns the password-change confirmation and replacement token pair for the current session.
/// </summary>
public class ChangePasswordResponseDto
{
    /// <summary>
    /// Gets or sets the password-change confirmation message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the replacement token pair for the current session.
    /// </summary>
    public LoginResponseDto Login { get; set; }
}
