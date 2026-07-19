namespace Authentication.Application.Dtos.Authentications.Otp;

/// <summary>
/// Represents the OTP verification payload used by authentication helper methods.
/// </summary>
public class OtpVerificationRequestDto
{
    /// <summary>
    /// Gets or sets the OTP purpose, such as register or forgot-password.
    /// </summary>
    public string Purpose { get; set; }

    /// <summary>
    /// Gets or sets the normalised email address bound to the OTP.
    /// </summary>
    public string NormalizedEmail { get; set; }

    /// <summary>
    /// Gets or sets the submitted OTP value.
    /// </summary>
    public string Otp { get; set; }
}
