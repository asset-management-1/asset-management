namespace Authentication.Application.Dtos.Authentications.Otp;

/// <summary>
/// Represents the result of sending an OTP email.
/// </summary>
public class OtpEmailResponseDto
{
    /// <summary>
    /// Gets or sets whether the OTP email was sent.
    /// </summary>
    public bool IsSuccess { get; set; }
}
