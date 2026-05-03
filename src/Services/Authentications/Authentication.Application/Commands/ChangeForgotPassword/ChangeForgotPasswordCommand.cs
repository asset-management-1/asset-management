namespace Authentication.Application.Commands.ChangeForgotPassword;

/// <summary>
/// Represents a request to change password after forgot-password OTP verification.
/// </summary>
public class ChangeForgotPasswordCommand : ICommand<ResponseDto<string>>
{
    /// <summary>
    /// Email address of the account that completed OTP verification.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// New password to be set for the account.
    /// </summary>
    public string NewPassword { get; set; }

    /// <summary>
    /// Confirmation value for <see cref="NewPassword"/>.
    /// </summary>
    public string ConfirmPassword { get; set; }
}
