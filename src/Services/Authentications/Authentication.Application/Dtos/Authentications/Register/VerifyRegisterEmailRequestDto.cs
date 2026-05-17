namespace Authentication.Application.Dtos.Authentications.Register;

/// <summary>
/// Represents the register-email verification payload used by the authentication service.
/// </summary>
public class VerifyRegisterEmailRequestDto
{
    public string Email { get; set; }
    public string Otp { get; set; }
}
