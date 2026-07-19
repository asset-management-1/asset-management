namespace Authentication.Application.Dtos.Authentications.Passwords;

/// <summary>
/// Represents the short-lived reset session stored after forgot-password OTP verification succeeds.
/// </summary>
public class ResetSessionCacheResponseDto
{
    /// <summary>
    /// Gets or sets the normalised email authorised for password reset.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the UTC expiration of the reset authority.
    /// </summary>
    public DateTime ExpiresAtUtc { get; set; }
}
