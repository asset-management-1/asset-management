namespace Authentication.Application.Dtos.Users.Profiles;

/// <summary>
/// Represents cached pending state and OTP data for a change-email request.
/// </summary>
public class ChangeEmailSessionCacheResponseDto
{
    /// <summary>
    /// Gets or sets the normalised new email address awaiting OTP verification.
    /// </summary>
    public string NewEmail { get; set; }

    /// <summary>
    /// Gets or sets the OTP state for the pending email change.
    /// </summary>
    public OtpCacheResponseDto Otp { get; set; }
}
