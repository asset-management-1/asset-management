namespace Be.Haven.Core.Jobs;

/// <summary>
/// Base Quartz job that provides:
/// 1) Configuration-based enable/disable.
/// 2) Optional distributed locking (to prevent the same job from running concurrently across multiple instances).
/// 3) A consistent logging pattern using <see cref="ILoggerFactory"/> (Sonar-friendly).
///
/// Derived jobs only need to:
/// - Provide <see cref="ConfigKey"/> (matches the key in configuration and attribute key if you use it).
/// - Implement <see cref="ExecuteInternalAsync"/> for the actual job logic.
/// </summary>
public abstract class BaseQuartzJob : IJob
{
    private readonly IDistributedLockService _lock;
    private readonly IOptionsMonitor<QuartzJobsOptions> _jobs;

    protected ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseQuartzJob"/> class.
    /// </summary>
    /// <param name="loggerFactory">Factory used to create a logger for the runtime job type (avoids Sonar warnings).</param>
    /// <param name="jobs">Options monitor holding Quartz job scheduling configuration.</param>
    /// <param name="lockService">
    /// Optional distributed lock service. If provided and <see cref="UseDistributedLock"/> is true,
    /// the job will run only when a lock can be acquired.
    /// </param>
    protected BaseQuartzJob(
        ILoggerFactory loggerFactory,
        IOptionsMonitor<QuartzJobsOptions> jobs,
        IDistributedLockService lockService = null)
    {
        Logger = loggerFactory.CreateLogger(GetType());
        _jobs = jobs;
        _lock = lockService;
    }

    /// <summary>
    /// The configuration key for this job (must match QuartzJobsOptions.Jobs[ConfigKey]).
    /// </summary>
    protected abstract string ConfigKey { get; }

    /// <summary>
    /// Enables/disables distributed locking for this job.
    /// Default is true to prevent multi-instance duplicate execution.
    /// </summary>
    protected virtual bool UseDistributedLock => true;

    /// <summary>
    /// The distributed lock key used in Redis (or another lock provider).
    /// ServicePrefix avoids lock key collisions across different microservices.
    /// </summary>
    protected virtual string LockKey => $"quartz:{_jobs.CurrentValue.ServicePrefix}:{ConfigKey}";

    /// <summary>
    /// Default lock TTL to avoid stale locks in case an instance crashes.
    /// Can be overridden or configured via QuartzJobConfigOptions.LockTtlSeconds.
    /// </summary>
    protected virtual TimeSpan DefaultLockTtl => TimeSpan.FromMinutes(5);

    /// <summary>
    /// Reads the job configuration from options. Returns null if not found.
    /// </summary>
    private QuartzJobConfigOptions Config =>
        _jobs.CurrentValue.Jobs.TryGetValue(ConfigKey, out var cfg) ? cfg : null;

    /// <summary>
    /// Resolves the lock TTL from configuration. Falls back to <see cref="DefaultLockTtl"/>.
    /// </summary>
    private TimeSpan ResolveLockTtl()
        => Config?.LockTtlSeconds is > 0
            ? TimeSpan.FromSeconds(Config.LockTtlSeconds.Value)
            : DefaultLockTtl;

    /// <summary>
    /// Resolves the lock wait timeout from configuration.
    /// 0 means "try once" and skip immediately if lock is held.
    /// </summary>
    private TimeSpan ResolveLockWaitTimeout()
        => Config?.LockWaitSeconds is > 0
            ? TimeSpan.FromSeconds(Config.LockWaitSeconds.Value)
            : DEFAULT_LOCK_WAIT_TIMEOUT;

    /// <summary>
    /// Resolves the lock polling delay from configuration.
    /// Used only when <see cref="ResolveLockWaitTimeout"/> is greater than 0.
    /// </summary>
    private TimeSpan ResolveLockPollDelay()
        => Config?.LockPollSeconds is > 0
            ? TimeSpan.FromSeconds(Config.LockPollSeconds.Value)
            : DEFAULT_LOCK_POLL_DELAY;

    /// <summary>
    /// Derived jobs implement their actual work here.
    /// </summary>
    /// <param name="context">Provides context information about the current job execution.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected abstract Task ExecuteInternalAsync(IJobExecutionContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Quartz entrypoint. Applies:
    /// - Config enable/disable
    /// - Optional distributed lock
    /// - Then executes job logic
    /// </summary>
    /// <param name="context">
    /// The execution context provided by Quartz, containing details about the job execution and a
    /// cancellation token to handle task cancellation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation of executing the job.
    /// </returns>
    public async Task Execute(IJobExecutionContext context)
    {
        var cfg = Config;

        // 1) Job disabled via configuration → skip execution
        if (cfg is not null && !cfg.Enabled)
        {
            Logger.LogInformation(QuartzLogs.LOG_JOB_DISABLED_BY_CONFIGURATION, ConfigKey);
            return;
        }

        var ct = context.CancellationToken;
        var correlationId = Guid.NewGuid().ToString("D");
        var currentActivity = Activity.Current;

        using (LogContext.PushProperty(CORRELATION_ID_PROPERTY, correlationId))
        {
            // Attach correlationId to OpenTelemetry trace (for distributed tracing)
            currentActivity?.SetTag(OTEL_CORRELATION_ID_TAG, correlationId);
            currentActivity?.AddBaggage(OTEL_CORRELATION_ID_TAG, correlationId);

            var lockKey = LockKey;
            var ttl = ResolveLockTtl();
            IAsyncDisposable handle = null;

            // 2) Job triggered by scheduler
            Logger.LogInformation(
                QuartzLogs.LOG_JOB_EXECUTION_REQUESTED,
                ConfigKey,
                Environment.MachineName,
                lockKey);

            try
            {
                // 3) Try acquire distributed lock
                handle = await AcquireLockAsync(lockKey, ttl, ct);

                // 4) Lock not acquired → another pod is running → skip
                if (UseDistributedLock && _lock is not null && handle is null)
                {
                    Logger.LogInformation(
                        QuartzLogs.LOG_JOB_SKIPPED_LOCK_HELD,
                        ConfigKey,
                        Environment.MachineName,
                        lockKey);
                    return;
                }

                // 5) Lock acquired → this pod will execute the job
                Logger.LogInformation(
                    QuartzLogs.LOG_JOB_LOCK_ACQUIRED,
                    ConfigKey,
                    Environment.MachineName,
                    lockKey);

                // 6) Execute actual job logic
                await ExecuteInternalAsync(context, ct);

                // 7) Job completed successfully
                Logger.LogInformation(
                    QuartzLogs.LOG_JOB_EXECUTION_COMPLETED,
                    ConfigKey,
                    Environment.MachineName);
            }
            // 8) App shutdown → cancellation is expected (not an error)
            catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
            {
                Logger.LogInformation(
                    ex,
                    QuartzLogs.LOG_JOB_CANCELLED_DURING_SHUTDOWN,
                    ConfigKey,
                    Environment.MachineName);
            }
            // 9) Unexpected error → log + rethrow (Quartz will mark job failed)
            catch (Exception ex)
            {
                Logger.LogError(
                    ex,
                    QuartzLogs.LOG_JOB_EXECUTION_FAILED,
                    ConfigKey,
                    Environment.MachineName);
            }
            finally
            {
                // 10) Always release lock if acquired
                if (handle is not null)
                {
                    await handle.DisposeAsync();
                }
            }
        }
    }

    /// <summary>
    /// Attempts to acquire a distributed lock for this job.
    ///
    /// Behavior:
    /// - If distributed locking is disabled (or lock service not registered), returns a no-op handle.
    /// - If LockWaitTimeout is 0 (or not configured), tries once and returns the result (null => skip).
    /// - Otherwise, repeatedly polls (TryAcquire) until:
    ///     a) lock is acquired  -> returns handle
    ///     b) wait timeout hit  -> returns null (caller should skip)
    ///     c) cancellation      -> returns null
    ///
    /// </summary>
    /// <param name="key">The unique key identifying the lock to be acquired.</param>
    /// <param name="ttl">The time-to-live duration for the lock, after which the lock will automatically expire.</param>
    /// <param name="ct">The cancellation token for aborting the lock acquisition process if required.</param>
    /// <returns>
    /// An <see cref="IAsyncDisposable"/> instance representing the lock handle if successfully acquired;
    /// <c>null</c> if the lock could not be acquired within the allowed time window.
    /// </returns>
    private async Task<IAsyncDisposable> AcquireLockAsync(string key, TimeSpan ttl, CancellationToken ct)
    {
        // 1) Distributed lock disabled → always allow execution (no-op lock)
        if (!UseDistributedLock || _lock is null)
        {
            Logger.LogDebug(QuartzLogs.LOG_JOB_LOCK_DISABLED, key);
            return NoopLock.Instance;
        }

        var waitTimeout = ResolveLockWaitTimeout();

        // 2) Try-once mode (default)
        // - No waiting
        // - If lock is held → skip immediately (return null)
        if (waitTimeout <= TimeSpan.Zero)
        {
            var handle = await _lock.TryAcquireAsync(key, ttl, ct);

            if (handle is null)
            {
                // Lock is already held by another pod → caller should skip job
                Logger.LogInformation(
                    QuartzLogs.LOG_JOB_LOCK_TRY_ONCE_FAILED,
                    key,
                    (int)ttl.TotalSeconds);
            }

            return handle;
        }

        // 3) Wait mode (optional)
        // - Poll repeatedly until:
        //   + Lock acquired
        //   + Timeout reached
        //   + Cancellation requested
        var pollDelay = ResolveLockPollDelay();
        var deadlineUtc = DateTime.UtcNow.Add(waitTimeout);

        Logger.LogDebug(
            QuartzLogs.LOG_JOB_LOCK_WAITING_STARTED,
            key,
            (int)ttl.TotalSeconds,
            (int)waitTimeout.TotalSeconds,
            (int)pollDelay.TotalSeconds);

        var attempts = 0;

        while (!ct.IsCancellationRequested && DateTime.UtcNow <= deadlineUtc)
        {
            attempts++;

            var handle = await _lock.TryAcquireAsync(key, ttl, ct);

            if (handle is not null)
            {
                // Lock acquired after retry attempts
                Logger.LogDebug(
                    QuartzLogs.LOG_JOB_LOCK_ACQUIRED_AFTER_WAIT,
                    key,
                    attempts);

                return handle;
            }

            // Still locked → wait and retry
            Logger.LogDebug(
                QuartzLogs.LOG_JOB_LOCK_WAITING_POLL,
                key,
                attempts);

            await Task.Delay(pollDelay, ct);
        }

        // 4) Cancelled while waiting (e.g., app shutdown)
        if (ct.IsCancellationRequested)
        {
            Logger.LogInformation(
                QuartzLogs.LOG_JOB_LOCK_WAITING_CANCELLED,
                key,
                attempts);

            return null;
        }

        // 5) Timeout reached → still cannot acquire lock
        Logger.LogInformation(
            QuartzLogs.LOG_JOB_LOCK_WAITING_TIMEOUT,
            key,
            attempts,
            (int)waitTimeout.TotalSeconds);

        return null;
    }

    /// <summary>
    /// No-op lock handle used when distributed locking is disabled.
    /// Ensures the "await using" pattern always works without null checks.
    /// </summary>
    private sealed class NoopLock : IAsyncDisposable
    {
        public static readonly NoopLock Instance = new();
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}