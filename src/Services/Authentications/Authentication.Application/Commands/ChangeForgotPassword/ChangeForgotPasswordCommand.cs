namespace Authentication.Application.Commands.ChangeForgotPassword;

/// <summary>
/// Represents a request to change password after forgot-password OTP verification.
/// </summary>
public class ChangeForgotPasswordCommand : ICommand<ResponseDto<OperationStatusResponseDto>>
{
    /// <summary>
    /// Opaque authority returned after successful forgot-password OTP verification.
    /// </summary>
    public string PasswordResetToken { get; set; }

    /// <summary>
    /// New password to be set for the account.
    /// </summary>
    public string NewPassword { get; set; }

}
