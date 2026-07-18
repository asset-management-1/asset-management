namespace Be.Haven.Tests.Shared.Be.Haven.Core.Jobs;

public sealed class BaseQuartzJobTests
{
    [Fact]
    public async Task Execute_Should_SkipExecution_When_JobIsDisabled()
    {
        // Arrange
        var sut = CreateJob(CreateOptions(enabled: false));

        // Act
        await sut.Execute(CreateContext());

        // Assert
        sut.ExecutionCount.Should().Be(0);
    }

    [Fact]
    public async Task Execute_Should_Throw_When_RequiredLockServiceIsMissing()
    {
        // Arrange
        var sut = CreateJob(CreateOptions(enabled: true));

        // Act
        var action = () => sut.Execute(CreateContext());

        // Assert
        await action.Should().ThrowAsync<DistributedLockUnavailableException>();
        sut.ExecutionCount.Should().Be(0);
    }

    [Fact]
    public async Task Execute_Should_RunWithoutLock_When_ConcreteJobOptsOut()
    {
        // Arrange
        var sut = CreateJob(CreateOptions(enabled: true));
        sut.RequiresDistributedLock = false;

        // Act
        await sut.Execute(CreateContext());

        // Assert
        sut.ExecutionCount.Should().Be(1);
    }

    [Fact]
    public async Task Execute_Should_SkipExecution_When_DistributedLockIsHeld()
    {
        // Arrange
        var lockService = new FakeDistributedLockService(null);
        var sut = CreateJob(
            CreateOptions(enabled: true),
            lockService);

        // Act
        await sut.Execute(CreateContext());

        // Assert
        sut.ExecutionCount.Should().Be(0);
        lockService.AttemptCount.Should().Be(1);
    }

    [Fact]
    public async Task Execute_Should_RunAndDisposeLock_When_DistributedLockIsAcquired()
    {
        // Arrange
        var handle = new FakeAsyncDisposable();
        var lockService = new FakeDistributedLockService(handle);
        var sut = CreateJob(
            CreateOptions(enabled: true, lockLeaseSeconds: 12),
            lockService);

        // Act
        await sut.Execute(CreateContext());

        // Assert
        sut.ExecutionCount.Should().Be(1);
        lockService.LastKey.Should().Be("quartz:core-service:core-job");
        lockService.LastLeaseDuration.Should().Be(TimeSpan.FromSeconds(12));
        handle.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_Should_WaitAndRun_When_LockBecomesAvailableBeforeTimeout()
    {
        // Arrange
        var handle = new FakeAsyncDisposable();
        var lockService = new DelayedDistributedLockService(handle, acquireOnAttempt: 1);
        var sut = CreateJob(
            CreateOptions(
                enabled: true,
                lockLeaseSeconds: 12,
                lockWaitSeconds: 1,
                lockPollSeconds: 1),
            lockService);

        // Act
        await sut.Execute(CreateContext());

        // Assert
        sut.ExecutionCount.Should().Be(1);
        lockService.AttemptCount.Should().Be(1);
        handle.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_Should_SkipExecution_When_LockWaitTimesOut()
    {
        // Arrange
        var lockService = new DelayedDistributedLockService(null, acquireOnAttempt: 99);
        var sut = CreateJob(
            CreateOptions(
                enabled: true,
                lockLeaseSeconds: 12,
                lockWaitSeconds: 1,
                lockPollSeconds: 1),
            lockService);

        // Act
        await sut.Execute(CreateContext());

        // Assert
        sut.ExecutionCount.Should().Be(0);
        lockService.AttemptCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Execute_Should_NotDelayPastLockWait_When_PollDelayIsLonger()
    {
        // Arrange
        var lockService = new DelayedDistributedLockService(null, acquireOnAttempt: 99);
        var sut = CreateJob(
            CreateOptions(
                enabled: true,
                lockLeaseSeconds: 12,
                lockWaitSeconds: 1,
                lockPollSeconds: 10),
            lockService);
        var stopwatch = Stopwatch.StartNew();

        // Act
        await sut.Execute(CreateContext());

        // Assert
        stopwatch.Stop();
        sut.ExecutionCount.Should().Be(0);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Execute_Should_SkipExecution_When_LockWaitIsCancelled()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var lockService = new DelayedDistributedLockService(null, acquireOnAttempt: 99);
        var sut = CreateJob(
            CreateOptions(
                enabled: true,
                lockLeaseSeconds: 12,
                lockWaitSeconds: 1,
                lockPollSeconds: 1),
            lockService);

        // Act
        await sut.Execute(CreateContext(cancellationTokenSource.Token));

        // Assert
        sut.ExecutionCount.Should().Be(0);
        lockService.AttemptCount.Should().Be(0);
    }

    [Fact]
    public async Task Execute_Should_RethrowExecutionException_When_InternalJobFails()
    {
        // Arrange
        var handle = new FakeAsyncDisposable();
        var lockService = new FakeDistributedLockService(handle);
        var sut = CreateJob(
            CreateOptions(enabled: true),
            lockService,
            throwOnExecute: true);

        // Act
        var action = () => sut.Execute(CreateContext());

        // Assert
        var exception = await action.Should().ThrowAsync<JobExecutionException>();
        exception.Which.InnerException.Should().BeOfType<InvalidOperationException>();
        sut.ExecutionCount.Should().Be(1);
        handle.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_Should_HandleCancellation_When_InternalJobIsCanceledDuringShutdown()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var handle = new FakeAsyncDisposable();
        var lockService = new FakeDistributedLockService(handle);
        var sut = CreateJob(
            CreateOptions(enabled: true),
            lockService,
            throwOnCancel: true);

        // Act
        await sut.Execute(CreateContext(cancellationTokenSource.Token));

        // Assert
        sut.ExecutionCount.Should().Be(1);
        handle.IsDisposed.Should().BeTrue();
    }

    private static CoreQuartzSampleJob CreateJob(
        QuartzJobsOptions options,
        IDistributedLockService lockService = null,
        bool throwOnExecute = false,
        bool throwOnCancel = false)
    {
        var monitor = new Mock<IOptionsMonitor<QuartzJobsOptions>>();
        monitor.SetupGet(x => x.CurrentValue).Returns(options);

        var result = new CoreQuartzSampleJob(
            LoggerFactory.Create(_ => { }),
            monitor.Object,
            lockService);
        result.ThrowOnExecute = throwOnExecute;
        result.ThrowOnCancel = throwOnCancel;

        return result;
    }

    private static QuartzJobsOptions CreateOptions(
        bool enabled,
        int? lockLeaseSeconds = null,
        int? lockWaitSeconds = null,
        int? lockPollSeconds = null) =>
        new()
        {
            ServicePrefix = "core-service",
            Jobs =
            {
                ["core-job"] = new QuartzJobConfigOptions
                {
                    Enabled = enabled,
                    LockLeaseSeconds = lockLeaseSeconds,
                    LockWaitSeconds = lockWaitSeconds,
                    LockPollSeconds = lockPollSeconds
                }
            }
        };

    private static IJobExecutionContext CreateContext(CancellationToken cancellationToken = default)
    {
        var context = new Mock<IJobExecutionContext>();
        context.SetupGet(x => x.CancellationToken).Returns(cancellationToken);

        return context.Object;
    }

    private sealed class CoreQuartzSampleJob : BaseQuartzJob
    {
        public CoreQuartzSampleJob(
            ILoggerFactory loggerFactory,
            IOptionsMonitor<QuartzJobsOptions> jobs,
            IDistributedLockService lockService)
            : base(loggerFactory, jobs, lockService)
        {
        }

        public int ExecutionCount { get; private set; }

        public bool ThrowOnExecute { get; set; }

        public bool ThrowOnCancel { get; set; }

        public bool RequiresDistributedLock { get; set; } = true;

        protected override string ConfigKey => "core-job";

        protected override bool UseDistributedLock => RequiresDistributedLock;

        protected override Task ExecuteInternalAsync(
            IJobExecutionContext context,
            CancellationToken cancellationToken)
        {
            ExecutionCount++;
            if (ThrowOnCancel)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            if (ThrowOnExecute)
            {
                throw new InvalidOperationException("job failed");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class DelayedDistributedLockService : IDistributedLockService
    {
        private readonly IAsyncDisposable _handle;
        private readonly int _acquireOnAttempt;

        public DelayedDistributedLockService(
            IAsyncDisposable handle,
            int acquireOnAttempt)
        {
            _handle = handle;
            _acquireOnAttempt = acquireOnAttempt;
        }

        public int AttemptCount { get; private set; }

        public Task<IAsyncDisposable> TryAcquireAsync(
            string key,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken)
        {
            AttemptCount++;

            return Task.FromResult(AttemptCount >= _acquireOnAttempt ? _handle : null);
        }
    }
}
