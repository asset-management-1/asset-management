namespace Authentication.Application.Commands.VerifyForgotPasswordOtp;

/// <summary>
/// Represents a request to verify forgot-password OTP.
/// </summary>
public class VerifyForgotPasswordOtpCommand : ICommand<ResponseDto<OperationStatusResponseDto>>
{
    /// <summary>
    /// Email address that received the OTP.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// OTP value provided by the user.
    /// </summary>
    public string Otp { get; set; }
}
