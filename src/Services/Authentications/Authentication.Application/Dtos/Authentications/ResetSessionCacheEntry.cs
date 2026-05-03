namespace Authentication.Application.Dtos.Authentications;

/// <summary>
/// Represents the short-lived reset session stored after forgot-password OTP verification succeeds.
/// </summary>
public class ResetSessionCacheEntry
{
    /// <summary>
    /// Gets or sets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
