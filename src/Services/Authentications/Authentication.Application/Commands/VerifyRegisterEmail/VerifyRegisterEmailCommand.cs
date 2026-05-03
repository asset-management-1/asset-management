namespace Authentication.Application.Commands.VerifyRegisterEmail;

/// <summary>
/// Represents a request to verify register email OTP.
/// </summary>
public class VerifyRegisterEmailCommand : ICommand<ResponseDto<string>>
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
