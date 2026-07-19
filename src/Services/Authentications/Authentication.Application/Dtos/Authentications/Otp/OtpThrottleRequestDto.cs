namespace Authentication.Application.Dtos.Authentications.Otp;

/// <summary>
/// Represents the OTP throttle-check payload for a specific purpose and email address.
/// </summary>
public class OtpThrottleRequestDto
{
    /// <summary>
    /// Gets or sets the OTP purpose, such as register or forgot-password.
    /// </summary>
    public string Purpose { get; set; }

    /// <summary>
    /// Gets or sets the normalised email address.
    /// </summary>
    public string NormalizedEmail { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed request count in the current window.
    /// </summary>
    public int Limit { get; set; }
}
