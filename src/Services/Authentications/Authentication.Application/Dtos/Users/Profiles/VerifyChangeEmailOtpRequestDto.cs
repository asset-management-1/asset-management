namespace Authentication.Application.Dtos.Users.Profiles;

/// <summary>
/// Represents a request to verify the OTP for a pending email change.
/// </summary>
public class VerifyChangeEmailOtpRequestDto
{
    /// <summary>
    /// Gets or sets the new email address being verified.
    /// </summary>
    public string NewEmail { get; set; }

    /// <summary>
    /// Gets or sets the OTP received at the new email address.
    /// </summary>
    public string Otp { get; set; }
}
