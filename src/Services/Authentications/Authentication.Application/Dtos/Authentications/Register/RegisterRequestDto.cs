namespace Authentication.Application.Dtos.Authentications.Register;

/// <summary>
/// Represents the register payload used by the authentication service.
/// </summary>
public class RegisterRequestDto
{
    public string UserName { get; set; }
    public string PartyType { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string FullName { get; set; }
}
