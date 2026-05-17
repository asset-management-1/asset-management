namespace Authentication.Application.Dtos.Authentications.Otp;

/// <summary>
/// Represents the OTP cache-write payload used when storing a newly generated OTP.
/// </summary>
public class OtpCacheRequestDto
{
    /// <summary>
    /// Gets or sets the OTP purpose, such as register or forgot-password.
    /// </summary>
    public string Purpose { get; set; }

    /// <summary>
    /// Gets or sets the normalized email address bound to the OTP.
    /// </summary>
    public string NormalizedEmail { get; set; }

    /// <summary>
    /// Gets or sets the generated OTP code.
    /// </summary>
    public string OtpCode { get; set; }
}
