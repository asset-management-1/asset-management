namespace Authentication.Application.Dtos.Authentications;

/// <summary>
/// Represents the cached OTP payload stored in Redis.
/// </summary>
public class OtpCacheEntry
{
    /// <summary>
    /// Gets or sets the OTP code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the number of failed verification attempts.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Gets or sets the UTC expiration timestamp used to preserve the original TTL.
    /// </summary>
    public DateTime ExpiresAtUtc { get; set; }
}
