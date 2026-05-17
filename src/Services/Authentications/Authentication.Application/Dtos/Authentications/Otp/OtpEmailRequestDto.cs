namespace Authentication.Application.Dtos.Authentications.Otp;

/// <summary>
/// Represents the OTP email payload sent through the shared email service.
/// </summary>
public class OtpEmailRequestDto
{
    /// <summary>
    /// Gets or sets the recipient email address.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the OTP code to send.
    /// </summary>
    public string OtpCode { get; set; }

    /// <summary>
    /// Gets or sets the email subject.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets the auth purpose for logging and tracing.
    /// </summary>
    public string Purpose { get; set; }
}
