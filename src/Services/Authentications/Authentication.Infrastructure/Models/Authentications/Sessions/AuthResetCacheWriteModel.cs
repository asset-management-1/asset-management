namespace Authentication.Infrastructure.Models.Authentications.Sessions;

/// <summary>
/// Groups inputs required to write one auth-reset cache marker.
/// </summary>
internal sealed class AuthResetCacheWriteModel
{
    /// <summary>
    /// Gets or sets the public user identifier.
    /// </summary>
    public Guid UserPublicId { get; set; }

    /// <summary>
    /// Gets or sets the optional UTC auth reset timestamp.
    /// </summary>
    public DateTime? AuthResetAt { get; set; }

    /// <summary>
    /// Gets or sets the configured refresh-token lifetime in days.
    /// </summary>
    public int RefreshTokenDays { get; set; }
}
