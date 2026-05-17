namespace Authentication.Application.Commands.Login;

/// <summary>
/// Represents the request payload for the login command.
/// </summary>
public class LoginCommand : ICommand<ResponseDto<LoginResponseDto>>
{
    /// <summary>
    /// Gets or sets the username used for login.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the password used for login.
    /// </summary>
    public string Password { get; set; }
}
