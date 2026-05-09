namespace Be.Haven.Cache.Interfaces;

public interface ILoginAttemptService
{
    /// <summary>
    /// Throws an exception if the user is currently locked due to too many failed attempts.
    /// </summary>
    /// <param name="userName">The username to check for a lockout state.</param>
    /// <param name="ct">The token used to cancel the Redis operation.</param>
    /// <returns>A task that completes when the lockout check finishes.</returns>
    Task CheckAccountLockedAsync(string userName, CancellationToken ct);

    /// <summary>
    /// Increases the failed attempt counter for the given user and locks the user if the limit is exceeded.
    /// </summary>
    /// <param name="userName">The username whose failed-attempt counter should be updated.</param>
    /// <param name="ct">The token used to cancel the Redis operation.</param>
    /// <returns>A task that completes when the failed-attempt counter is updated.</returns>
    Task CountFailedAttemptAsync(string userName, CancellationToken ct);

    /// <summary>
    /// Removes the failed attempt counter and removes any active lock for the user.
    /// </summary>
    /// <param name="userName">The username whose lockout state should be removed.</param>
    /// <param name="ct">The token used to cancel the Redis operation.</param>
    /// <returns>A task that completes when the lockout state is removed.</returns>
    Task RemoveAttemptsAsync(string userName, CancellationToken ct);
}
