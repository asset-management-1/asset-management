namespace Be.Haven.Shared.Dtos.Options.Caching;

public class LoginAttemptOptions
{
    /// <summary>
    /// Maximum number of allowed failed login attempts before locking the user.
    /// </summary>
    public int MaxFailedAttempts { get; set; }

    /// <summary>
    /// The time window in which failed attempts are counted.
    /// After this time, the failure counter will expire automatically.
    /// </summary>
    public TimeSpan FailedWindow { get; set; }

    /// <summary>
    /// The duration for which the user will be locked after exceeding the max attempts.
    /// </summary>
    public TimeSpan LockDuration { get; set; }
}