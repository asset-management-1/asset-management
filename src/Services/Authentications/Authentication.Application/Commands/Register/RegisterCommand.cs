namespace Authentication.Application.Commands.Register;

/// <summary>
/// Represents the request payload for the register command.
/// </summary>
public class RegisterCommand : ICommand<ResponseDto<OperationStatusResponseDto>>
{
    /// <summary>
    /// Username used for account login.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Tenant or landlord party type. Accepted values: <c>Tenant</c>, <c>Landlord</c>.
    /// </summary>
    public PartyTypeEnum PartyType { get; set; }

    /// <summary>
    /// Plain text password for account creation. The API never returns this value.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// User email used for OTP delivery and account contact.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// User phone number.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// User full name.
    /// </summary>
    public string FullName { get; set; }
}
