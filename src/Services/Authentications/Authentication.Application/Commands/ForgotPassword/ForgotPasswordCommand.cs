namespace Authentication.Application.Commands.ForgotPassword;

public class ForgotPasswordCommand : ICommand<ResponseDto<string>>
{
    /// <summary>
    /// Username or email of account that requests password reset.
    /// </summary>
    public string UserNameOrEmail { get; set; }

    /// <summary>
    /// New password that will replace current account password.
    /// </summary>
    public string NewPassword { get; set; }
}
