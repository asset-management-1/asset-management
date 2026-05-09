namespace Be.Haven.Cache.Services;

/// <summary>
/// Provides a Redis-based implementation of <see cref="ILoginAttemptService"/>.
/// Uses Redis keys to track failed login attempts and lockout state across distributed instances.
/// </summary>
public class LoginAttemptService : ILoginAttemptService
{
    private readonly IDatabase _redis;
    private readonly ILogger<LoginAttemptService> _logger;
    private readonly LoginAttemptOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginAttemptService"/> class.
    /// </summary>
    /// <param name="connectionMultiplexer">The Redis connection multiplexer used to obtain a database instance.</param>
    /// <param name="logger">The logger used to record authentication and lockout events.</param>
    /// <param name="options">Configuration options defining login limits and lockout duration.</param>
    public LoginAttemptService(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<LoginAttemptService> logger,
        IOptions<LoginAttemptOptions> options)
    {
        _logger = logger;
        _redis = connectionMultiplexer.GetDatabase();
        _options = options.Value;
    }

    /// <summary>
    /// Ensures that the specified user is not currently locked out.
    /// If the user is locked, this method throws an exception to prevent further login attempts.
    /// </summary>
    /// <param name="userName">The username to check for a lockout state.</param>
    /// <param name="ct">A cancellation token used to cancel the operation.</param>
    /// <returns>A task that completes when the lockout check finishes.</returns>
    /// <exception cref="ArgumentException">Thrown when the user is currently locked out.</exception>
    public async Task CheckAccountLockedAsync(
        string userName,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return;
        }

        var lockKey = GetLockKey(userName);
        var isLocked = await _redis.KeyExistsAsync(lockKey);
        if (!isLocked)
        {
            return;
        }

        // Retrieve remaining TTL (time to live) for informational logging.
        var ttl = await _redis.KeyTimeToLiveAsync(lockKey);

        // Logs a warning indicating that the user is still locked out, along with the remaining lock duration.
        _logger.LogWarning(USER_LOCKED, userName, ttl?.TotalSeconds);

        throw new ArgumentException(ACCOUNT_LOCKED);
    }

    /// <summary>
    /// Increments the failed login attempt count for the specified user.
    /// If the number of failed attempts exceeds the configured threshold, the user is locked.
    /// </summary>
    /// <param name="userName">The username whose failed attempt count should be updated.</param>
    /// <param name="ct">A cancellation token used to cancel the operation.</param>
    /// <returns>A task that completes when the failed-attempt counter is updated.</returns>
    public async Task CountFailedAttemptAsync(
        string userName,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return;
        }

        var failKey = GetFailKey(userName);
        var lockKey = GetLockKey(userName);

        // Atomically increment the failed attempt counter in Redis.
        var failedCount = await _redis.StringIncrementAsync(failKey);

        // Set the counter TTL only on the first failed attempt so the window expires automatically.
        if (failedCount == 1)
        {
            await _redis.KeyExpireAsync(failKey, _options.FailedWindow);
        }

        _logger.LogInformation(
            USER_FAILED_ATTEMPT,
            userName,
            failedCount,
            _options.MaxFailedAttempts);

        // Create a separate lock key when the configured failed-attempt threshold is reached.
        if (failedCount >= _options.MaxFailedAttempts)
        {
            await _redis.StringSetAsync(lockKey, LOCKED, _options.LockDuration);

            _logger.LogWarning(
                USER_LOCK_CREATED,
                userName,
                _options.LockDuration.TotalMinutes,
                failedCount);
        }
    }

    /// <summary>
    /// Resets the failed attempt counter and removes any active lock for the given user.
    /// Typically invoked after a successful login attempt.
    /// </summary>
    /// <param name="userName">The username whose lock and failed attempts will be cleared.</param>
    /// <param name="ct">A cancellation token used to cancel the operation.</param>
    /// <returns>A task that completes when the lockout state is removed.</returns>
    public async Task RemoveAttemptsAsync(
        string userName,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return;
        }

        var failKey = GetFailKey(userName);
        var lockKey = GetLockKey(userName);

        await _redis.KeyDeleteAsync(failKey);
        await _redis.KeyDeleteAsync(lockKey);

        // Record that the user's failed-attempt and lockout state has been cleared.
        _logger.LogInformation(USER_RESET, userName);
    }

    /// <summary>
    /// Builds the Redis key for the user’s lock state.
    /// </summary>
    /// <param name="userName">The username to use in the lock key.</param>
    /// <returns>A formatted Redis key string.</returns>
    private static string GetLockKey(string userName)
    {
        return string.Format(LOCK_KEY_PREFIX, userName);
    }

    /// <summary>
    /// Builds the Redis key for tracking the user’s failed login attempts.
    /// </summary>
    /// <param name="userName">The username to use in the fail key.</param>
    /// <returns>A formatted Redis key string.</returns>
    private static string GetFailKey(string userName)
    {
        return string.Format(FAIL_KEY_PREFIX, userName);
    }
}
