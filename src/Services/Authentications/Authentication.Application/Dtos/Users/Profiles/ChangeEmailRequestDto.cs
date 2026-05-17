namespace Authentication.Application.Dtos.Users.Profiles;

/// <summary>
/// Represents a request to start changing the current user's email address.
/// </summary>
public class ChangeEmailRequestDto
{
    /// <summary>
    /// Gets or sets the new email address to verify.
    /// </summary>
    public string NewEmail { get; set; }
}
