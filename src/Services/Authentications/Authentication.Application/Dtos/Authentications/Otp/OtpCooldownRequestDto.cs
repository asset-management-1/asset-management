namespace Authentication.Application.Dtos.Authentications.Otp;

/// <summary>
/// Represents the OTP cooldown payload for a specific purpose and email address.
/// </summary>
public class OtpCooldownRequestDto
{
    /// <summary>
    /// Gets or sets the OTP purpose, such as register or forgot-password.
    /// </summary>
    public string Purpose { get; set; }

    /// <summary>
    /// Gets or sets the normalised email address.
    /// </summary>
    public string NormalizedEmail { get; set; }
}
