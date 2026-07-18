namespace Be.Haven.Core.Jobs;

/// <summary>
/// Provides configuration, distributed locking, tracing, and logging for Quartz jobs.
/// </summary>
public abstract class BaseQuartzJob : IJob
{
    private readonly IDistributedLockService _lockService;
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<QuartzJobsOptions> _options;

    /// <summary>
    /// Creates the shared Quartz job workflow.
    /// </summary>
    /// <param name="loggerFactory">The factory used to create a logger for the concrete job.</param>
    /// <param name="options">The monitor containing scheduling and lock settings.</param>
    /// <param name="lockService">The optional provider used to coordinate execution across instances.</param>
    protected BaseQuartzJob(
        ILoggerFactory loggerFactory,
        IOptionsMonitor<QuartzJobsOptions> options,
        IDistributedLockService lockService = null)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _options = options;
        _lockService = lockService;
    }

    /// <summary>
    /// Gets the key used to resolve this job from <see cref="QuartzJobsOptions"/>.
    /// </summary>
    protected abstract string ConfigKey { get; }

    /// <summary>
    /// Gets a value indicating whether this job requires cross-instance locking.
    /// </summary>
    protected virtual bool UseDistributedLock => true;

    /// <summary>
    /// Gets the namespaced lock key for this job.
    /// </summary>
    protected virtual string LockKey => $"quartz:{_options.CurrentValue.ServicePrefix}:{ConfigKey}";

    /// <summary>
    /// Gets the fallback lease used when the job has no configured lease duration.
    /// </summary>
    protected virtual TimeSpan DefaultLockLeaseDuration => TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets the current configuration for the concrete job when one has been registered.
    /// </summary>
    private QuartzJobConfigOptions Configuration =>
        _options.CurrentValue.Jobs.TryGetValue(ConfigKey, out var configuration)
            ? configuration
            : null;

    /// <summary>
    /// Executes the business work implemented by the concrete job.
    /// </summary>
    /// <param name="context">The current Quartz execution context.</param>
    /// <param name="cancellationToken">The token used to cancel the job.</param>
    /// <returns>A task representing the job operation.</returns>
    protected abstract Task ExecuteInternalAsync(
        IJobExecutionContext context,
        CancellationToken cancellationToken);

    /// <summary>
    /// Executes one configured and optionally locked Quartz job invocation.
    /// </summary>
    /// <param name="context">The current Quartz execution context.</param>
    /// <returns>A task representing the complete job invocation.</returns>
    public async Task Execute(IJobExecutionContext context)
    {
        // Step 1: Resolve the concrete job configuration before creating trace or lock state.
        var configuration = Configuration;

        if (configuration is not null && !configuration.Enabled)
        {
            _logger.LogInformation(QuartzLogs.LOG_JOB_DISABLED_BY_CONFIGURATION, ConfigKey);
            return;
        }

        var cancellationToken = context.CancellationToken;
        var correlationId = Guid.NewGuid().ToString("D");

        // Step 2: Attach one correlation identifier to the complete scheduled invocation.
        using (LogContext.PushProperty(CORRELATION_ID_PROPERTY, correlationId))
        {
            Activity.Current?.SetTag(OTEL_CORRELATION_ID_TAG, correlationId);
            Activity.Current?.AddBaggage(OTEL_CORRELATION_ID_TAG, correlationId);

            var lockKey = LockKey;
            var leaseDuration = ResolveLockLeaseDuration(configuration);
            var waitTimeout = ResolveLockWaitTimeout(configuration);
            var pollDelay = ResolveLockPollDelay(configuration);

            _logger.LogInformation(
                QuartzLogs.LOG_JOB_EXECUTION_REQUESTED,
                ConfigKey,
                Environment.MachineName,
                lockKey);

            try
            {
                // Step 3: Acquire the job lock before delegated work can observe or mutate shared state.
                var lockHandle = await AcquireLockAsync(
                    lockKey,
                    leaseDuration,
                    waitTimeout,
                    pollDelay,
                    cancellationToken);

                if (UseDistributedLock && lockHandle is null)
                {
                    _logger.LogInformation(
                        QuartzLogs.LOG_JOB_SKIPPED_LOCK_HELD,
                        ConfigKey,
                        Environment.MachineName,
                        lockKey);
                    return;
                }

                await using (lockHandle)
                {
                    if (UseDistributedLock)
                    {
                        _logger.LogInformation(
                            QuartzLogs.LOG_JOB_LOCK_ACQUIRED,
                            ConfigKey,
                            Environment.MachineName,
                            lockKey);
                    }

                    // Step 4: Run the concrete job while the acquired handle remains in scope.
                    await ExecuteInternalAsync(context, cancellationToken);

                    _logger.LogInformation(
                        QuartzLogs.LOG_JOB_EXECUTION_COMPLETED,
                        ConfigKey,
                        Environment.MachineName);
                }
            }
            catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
            {
                // Step 5a: Treat host shutdown cancellation as an expected lifecycle outcome.
                _logger.LogInformation(
                    exception,
                    QuartzLogs.LOG_JOB_CANCELLED_DURING_SHUTDOWN,
                    ConfigKey,
                    Environment.MachineName);
            }
            catch (Exception exception) when (exception is not DistributedLockUnavailableException)
            {
                // Step 5b: Add job context and let Quartz record the invocation as failed.
                throw new JobExecutionException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        ErrorConstants.QuartzErrors.QUARTZ_JOB_EXECUTION_FAILED,
                        ConfigKey,
                        Environment.MachineName),
                    exception);
            }
        }
    }

    /// <summary>
    /// Acquires the job lock immediately or polls within the configured wait window.
    /// </summary>
    /// <param name="key">The namespaced lock key.</param>
    /// <param name="leaseDuration">The lease maintained by the provider while the handle is held.</param>
    /// <param name="waitTimeout">The maximum time allowed for bounded lock polling.</param>
    /// <param name="pollDelay">The delay between lock attempts while bounded polling is active.</param>
    /// <param name="cancellationToken">The token used to cancel lock polling.</param>
    /// <returns>The acquired handle, or <c>null</c> when locking is disabled or another owner holds the lock.</returns>
    private async Task<IAsyncDisposable> AcquireLockAsync(
        string key,
        TimeSpan leaseDuration,
        TimeSpan waitTimeout,
        TimeSpan pollDelay,
        CancellationToken cancellationToken)
    {
        // Step 1: Skip provider acquisition when the concrete job explicitly opts out of coordination.
        if (!UseDistributedLock)
        {
            _logger.LogDebug(QuartzLogs.LOG_JOB_LOCK_DISABLED, key);
            return null;
        }

        if (_lockService is null)
        {
            throw new DistributedLockUnavailableException(
                new InvalidOperationException(
                    ErrorConstants.DistributedLockErrors.DISTRIBUTED_LOCK_UNAVAILABLE));
        }

        // Step 2: Try once by default so one busy instance does not delay the scheduler thread.
        if (waitTimeout <= TimeSpan.Zero)
        {
            var lockHandle = await _lockService.TryAcquireAsync(
                key,
                leaseDuration,
                cancellationToken);

            if (lockHandle is null)
            {
                _logger.LogInformation(
                    QuartzLogs.LOG_JOB_LOCK_TRY_ONCE_FAILED,
                    key,
                    (int)leaseDuration.TotalSeconds);
            }

            return lockHandle;
        }

        var deadlineUtc = DateTime.UtcNow.Add(waitTimeout);
        var attempts = 0;

        _logger.LogDebug(
            QuartzLogs.LOG_JOB_LOCK_WAITING_STARTED,
            key,
            (int)leaseDuration.TotalSeconds,
            (int)waitTimeout.TotalSeconds,
            (int)pollDelay.TotalSeconds);

        // Step 3: Poll only within the configured window; provider calls never wait internally on contention.
        while (!cancellationToken.IsCancellationRequested && DateTime.UtcNow <= deadlineUtc)
        {
            attempts++;

            var lockHandle = await _lockService.TryAcquireAsync(
                key,
                leaseDuration,
                cancellationToken);

            if (lockHandle is not null)
            {
                _logger.LogDebug(
                    QuartzLogs.LOG_JOB_LOCK_ACQUIRED_AFTER_WAIT,
                    key,
                    attempts);

                return lockHandle;
            }

            _logger.LogDebug(
                QuartzLogs.LOG_JOB_LOCK_WAITING_POLL,
                key,
                attempts);

            var remainingWait = deadlineUtc - DateTime.UtcNow;
            if (remainingWait <= TimeSpan.Zero)
            {
                break;
            }

            // Step 4: Cap the next delay so polling never exceeds the configured wait window.
            await Task.Delay(
                pollDelay < remainingWait ? pollDelay : remainingWait,
                cancellationToken);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            // Step 5a: Return without execution when host cancellation interrupts lock polling.
            _logger.LogInformation(
                QuartzLogs.LOG_JOB_LOCK_WAITING_CANCELLED,
                key,
                attempts);

            return null;
        }

        // Step 5b: Return contention after the configured wait window instead of running without ownership.
        _logger.LogInformation(
            QuartzLogs.LOG_JOB_LOCK_WAITING_TIMEOUT,
            key,
            attempts,
            (int)waitTimeout.TotalSeconds);

        return null;
    }

    /// <summary>
    /// Resolves the temporary lock-ownership lease for the current job.
    /// </summary>
    /// <param name="configuration">The current concrete job configuration, when registered.</param>
    /// <returns>The configured positive lease or the job fallback lease.</returns>
    private TimeSpan ResolveLockLeaseDuration(QuartzJobConfigOptions configuration)
    {
        return configuration?.LockLeaseSeconds is > 0
            ? TimeSpan.FromSeconds(configuration.LockLeaseSeconds.Value)
            : DefaultLockLeaseDuration;
    }

    /// <summary>
    /// Resolves how long the scheduler invocation may poll before skipping execution.
    /// </summary>
    /// <param name="configuration">The current concrete job configuration, when registered.</param>
    /// <returns>The configured positive wait duration or the immediate-attempt default.</returns>
    private static TimeSpan ResolveLockWaitTimeout(QuartzJobConfigOptions configuration)
    {
        return configuration?.LockWaitSeconds is > 0
            ? TimeSpan.FromSeconds(configuration.LockWaitSeconds.Value)
            : DEFAULT_LOCK_WAIT_TIMEOUT;
    }

    /// <summary>
    /// Resolves the delay between lock attempts while bounded waiting is enabled.
    /// </summary>
    /// <param name="configuration">The current concrete job configuration, when registered.</param>
    /// <returns>The configured positive polling delay or the shared default delay.</returns>
    private static TimeSpan ResolveLockPollDelay(QuartzJobConfigOptions configuration)
    {
        return configuration?.LockPollSeconds is > 0
            ? TimeSpan.FromSeconds(configuration.LockPollSeconds.Value)
            : DEFAULT_LOCK_POLL_DELAY;
    }

}
