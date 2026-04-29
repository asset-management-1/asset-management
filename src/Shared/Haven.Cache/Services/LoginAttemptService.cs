namespace Haven.Cache.Services;

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

        /// <summary>
        /// Logs a warning indicating that the user is still locked out,
        /// along with the remaining lock duration.
        /// </summary>
        _logger.LogWarning(USER_LOCKED, userName, ttl?.TotalSeconds);

        throw new ArgumentException(ACCOUNT_LOCKED);
    }

    /// <summary>
    /// Increments the failed login attempt count for the specified user.
    /// If the number of failed attempts exceeds the configured threshold, the user is locked.
    /// </summary>
    /// <param name="userName">The username whose failed attempt count should be updated.</param>
    /// <param name="ct">A cancellation token used to cancel the operation.</param>
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

        /// <summary>
        /// Atomically increments the failed attempt counter in Redis.
        /// </summary>
        var failedCount = await _redis.StringIncrementAsync(failKey);

        /// <summary>
        /// Sets a time-to-live for the failed attempt counter on the first failed attempt.
        /// Ensures that counters automatically reset after the configured window.
        /// </summary>
        if (failedCount == 1)
        {
            await _redis.KeyExpireAsync(failKey, _options.FailedWindow);
        }

        _logger.LogInformation(
            USER_FAILED_ATTEMPT,
            userName,
            failedCount,
            _options.MaxFailedAttempts);

        /// <summary>
        /// Locks the user if the number of failed attempts reaches the configured threshold.
        /// A separate Redis key is created to represent the lock state with an expiration time.
        /// </summary>
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

        /// <summary>
        /// Logs that the user’s login attempt data and lock state were successfully cleared.
        /// </summary>
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
