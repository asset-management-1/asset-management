namespace Authentication.Application.Dtos.Authentications.Register;

/// <summary>
/// Represents the register session cached until the email OTP is verified.
/// </summary>
public class RegisterSessionCacheRequestDto
{
    /// <summary>
    /// Gets or sets the pending register payload used to create the account after OTP verification.
    /// </summary>
    public PendingRegisterCacheRequestDto PendingRegister { get; set; }

    /// <summary>
    /// Gets or sets the OTP state for the pending register session.
    /// </summary>
    public OtpCacheResponseDto Otp { get; set; }
}
