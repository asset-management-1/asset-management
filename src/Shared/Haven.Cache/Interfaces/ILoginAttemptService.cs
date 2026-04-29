namespace Haven.Cache.Interfaces;

public interface ILoginAttemptService
{
    /// <summary>
    /// Throws an exception if the user is currently locked due to too many failed attempts.
    /// </summary>
    Task CheckAccountLockedAsync(string userName, CancellationToken ct);

    /// <summary>
    /// Increases the failed attempt counter for the given user and locks the user if the limit is exceeded.
    /// </summary>
    Task CountFailedAttemptAsync(string userName, CancellationToken ct);

    /// <summary>
    /// Remove the failed attempt counter and removes any active lock for the user.
    /// </summary>
    Task RemoveAttemptsAsync(string userName, CancellationToken ct);
}
