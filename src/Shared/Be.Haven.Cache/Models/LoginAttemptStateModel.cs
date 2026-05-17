namespace Be.Haven.Cache.Models;

/// <summary>
/// Represents in-memory login attempt state for one normalized user name.
/// </summary>
internal sealed class LoginAttemptStateModel
{
    /// <summary>
    /// Gets or sets the number of failed login attempts in the active window.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the first failed attempt in the active window.
    /// </summary>
    public DateTime FirstFailedUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp until which the user is locked, when locked.
    /// </summary>
    public DateTime? LockUntilUtc { get; set; }
}
