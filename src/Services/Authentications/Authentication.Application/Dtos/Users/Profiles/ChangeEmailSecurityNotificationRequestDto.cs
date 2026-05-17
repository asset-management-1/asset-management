namespace Authentication.Application.Dtos.Users.Profiles;

/// <summary>
/// Represents the security notification sent to the old email during change-email flow.
/// </summary>
public class ChangeEmailSecurityNotificationRequestDto
{
    /// <summary>
    /// Gets or sets the old email address that receives the security notification.
    /// </summary>
    public string OldEmail { get; set; }

    /// <summary>
    /// Gets or sets the new email address requested by the user.
    /// </summary>
    public string NewEmail { get; set; }
}
