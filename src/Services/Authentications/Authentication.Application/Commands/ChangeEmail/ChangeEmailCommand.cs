namespace Authentication.Application.Commands.ChangeEmail;

/// <summary>
/// Represents a request to start or resend change-email OTP verification.
/// </summary>
public class ChangeEmailCommand : ICommand<ResponseDto<OperationStatusResponseDto>>
{
    /// <summary>
    /// Gets or sets the new email address to verify.
    /// </summary>
    public string NewEmail { get; set; }
}
