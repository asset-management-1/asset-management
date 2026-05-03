namespace Authentication.Application.Commands.ForgotPassword;

/// <summary>
/// Represents a request to start the forgot-password OTP flow.
/// </summary>
public class ForgotPasswordCommand : ICommand<ResponseDto<string>>
{
    /// <summary>
    /// Email address of the account that requests password recovery.
    /// </summary>
    public string Email { get; set; }
}
