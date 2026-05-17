namespace Authentication.Application.Commands.VerifyChangeEmailOtp;

/// <summary>
/// Represents a request to verify and apply a pending email change.
/// </summary>
public class VerifyChangeEmailOtpCommand : ICommand<ResponseDto<OperationStatusResponseDto>>
{
    /// <summary>
    /// Gets or sets the new email address being verified.
    /// </summary>
    public string NewEmail { get; set; }

    /// <summary>
    /// Gets or sets the OTP received at the new email address.
    /// </summary>
    public string Otp { get; set; }
}
