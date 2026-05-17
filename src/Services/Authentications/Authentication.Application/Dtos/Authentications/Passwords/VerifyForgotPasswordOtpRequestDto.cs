namespace Authentication.Application.Dtos.Authentications.Passwords;

/// <summary>
/// Represents the forgot-password OTP verification payload used by the authentication service.
/// </summary>
public class VerifyForgotPasswordOtpRequestDto
{
    public string Email { get; set; }
    public string Otp { get; set; }
}
