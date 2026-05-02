namespace Authentication.Application.Commands.Register;

public class RegisterCommand : ICommand<ResponseDto<LoginResponse>>
{
    /// <summary>
    /// Username used for account login.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Plain text password for account creation.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Optional user email.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Optional user phone number.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// User full name.
    /// </summary>
    public string FullName { get; set; }
}
