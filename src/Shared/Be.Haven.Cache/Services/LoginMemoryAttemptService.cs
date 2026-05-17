namespace Be.Haven.Cache.Services;

/// <summary>
/// Provides an in-memory implementation of <see cref="ILoginAttemptService"/>.
/// Used when Redis is not available (for local development or testing).
/// Stores login attempt data in a concurrent dictionary within the application process.
/// </summary>
public class LoginMemoryAttemptService : ILoginAttemptService
{
    private readonly ILogger<LoginMemoryAttemptService> _logger;
    private readonly LoginAttemptOptions _options;

    /// <summary>
    /// Stores the login attempt state for each user in memory.
    /// Key = username, Value = <see cref="LoginAttemptStateModel"/> representing the user's attempt info.
    /// </summary>
    private static readonly ConcurrentDictionary<string, LoginAttemptStateModel> _attempts = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginMemoryAttemptService"/> class.
    /// </summary>
    /// <param name="logger">Used for logging lockout and attempt information.</param>
    /// <param name="options">Provides configuration for max attempts, lock duration, and window period.</param>
    public LoginMemoryAttemptService(
        ILogger<LoginMemoryAttemptService> logger,
        IOptions<LoginAttemptOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Ensures the user is not currently locked. If locked, throws HTTP 429.
    /// If the lock has expired, resets the user's state.
    /// </summary>
    /// <param name="userName">The username to check for a lockout state.</param>
    /// <param name="ct">The token used to cancel the operation.</param>
    /// <returns>A task that completes when the lockout check finishes.</returns>
    /// <exception cref="HttpStatusCodeException">Thrown when the user is currently locked.</exception>
    public Task CheckAccountLockedAsync(
        string userName,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return Task.CompletedTask;
        }

        if (!_attempts.TryGetValue(userName, out var state))
        {
            return Task.CompletedTask;
        }

        var now = DateTime.UtcNow;

        // Check if the user is still locked
        if (state.LockUntilUtc.HasValue && state.LockUntilUtc.Value > now)
        {
            var remaining = state.LockUntilUtc.Value - now;

            _logger.LogWarning(
                USER_LOCKED,
                userName,
                remaining.TotalSeconds);

            throw new HttpStatusCodeException(
                ACCOUNT_LOCKED,
                AppConstants.SystemMessageCode.MANY_REQUESTS,
                AppConstants.SystemCode.MANY_REQUESTS);
        }

        // If the lock has expired, clear lock info and reset failed count
        if (state.LockUntilUtc.HasValue && state.LockUntilUtc.Value <= now)
        {
            state.LockUntilUtc = null;
            state.FailedCount = 0;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Registers a failed login attempt for a specific user.
    /// If the number of failed attempts exceeds the limit, the user will be locked.
    /// </summary>
    /// <param name="userName">The username whose failed-attempt counter should be updated.</param>
    /// <param name="ct">The token used to cancel the operation.</param>
    /// <returns>A task that completes when the failed-attempt counter is updated.</returns>
    public Task CountFailedAttemptAsync(
        string userName,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return Task.CompletedTask;
        }

        var now = DateTime.UtcNow;

        // Retrieve or create attempt state for the user
        var state = _attempts.GetOrAdd(userName, _ => new LoginAttemptStateModel
        {
            FailedCount = 0,
            FirstFailedUtc = now,
            LockUntilUtc = null
        });

        lock (state)
        {
            // If the current time exceeds the failed window duration, reset counters
            if (now - state.FirstFailedUtc > _options.FailedWindow)
            {
                state.FailedCount = 0;
                state.FirstFailedUtc = now;
            }

            // Increment failed attempt count
            state.FailedCount++;

            _logger.LogInformation(
                USER_FAILED_ATTEMPT,
                userName,
                state.FailedCount,
                _options.MaxFailedAttempts);

            // Lock the user if the failed count exceeds the limit
            if (state.FailedCount >= _options.MaxFailedAttempts)
            {
                state.LockUntilUtc = now.Add(_options.LockDuration);

                _logger.LogWarning(
                    USER_LOCK_CREATED,
                    userName,
                    _options.LockDuration.TotalMinutes,
                    state.FailedCount);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Resets all login attempt data and removes any active lock for the given user.
    /// </summary>
    /// <param name="userName">The username whose lockout state should be removed.</param>
    /// <param name="ct">The token used to cancel the operation.</param>
    /// <returns>A task that completes when the lockout state is removed.</returns>
    public Task RemoveAttemptsAsync(
        string userName,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return Task.CompletedTask;
        }

        // Remove the user's attempt state entirely from memory
        _attempts.TryRemove(userName, out _);

        _logger.LogInformation(
            USER_RESET,
            userName);

        return Task.CompletedTask;
    }
}
