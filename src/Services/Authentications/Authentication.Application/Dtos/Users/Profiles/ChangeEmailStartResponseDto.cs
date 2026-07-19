namespace Authentication.Application.Dtos.Users.Profiles;

/// <summary>
/// Represents validated data needed to send change-email notifications.
/// </summary>
public class ChangeEmailStartResponseDto
{
    /// <summary>
    /// Gets or sets the email address currently stored on the account.
    /// </summary>
    public string OldEmail { get; set; }

    /// <summary>
    /// Gets or sets the normalised new email address.
    /// </summary>
    public string NewEmail { get; set; }
}
