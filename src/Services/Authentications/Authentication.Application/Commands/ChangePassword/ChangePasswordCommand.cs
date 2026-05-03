namespace Authentication.Application.Commands.ChangePassword;

/// <summary>
/// Represents a request to change password for the current authenticated user.
/// </summary>
public class ChangePasswordCommand : ICommand<ResponseDto<string>>
{
    /// <summary>
    /// Current password of the authenticated user.
    /// </summary>
    public string CurrentPassword { get; set; }

    /// <summary>
    /// New password to be set.
    /// </summary>
    public string NewPassword { get; set; }

    /// <summary>
    /// Confirmation value for <see cref="NewPassword"/>.
    /// </summary>
    public string ConfirmPassword { get; set; }
}
