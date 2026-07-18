namespace Be.Haven.Shared.Dtos.Options.Scheduling;

public class QuartzJobConfigOptions
{
    /// <summary>
    /// Determines whether the Quartz job is enabled or disabled in the configuration.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Specifies the cron expression used to define the schedule for a Quartz job.
    /// The cron expression controls the timing and frequency at which the job will execute.
    /// </summary>
    public string CronExpression { get; set; }

    /// <summary>
    /// Specifies the interval in seconds for executing a Quartz job when a simple interval trigger is used.
    /// If not specified, the scheduler applies a 300-second default interval.
    /// </summary>
    public int? IntervalSeconds { get; set; }

    /// <summary>
    /// Specifies the initial lease duration for the distributed lock associated with the Quartz job.
    /// A lease represents temporary lock ownership, not cache-data lifetime or maximum job runtime.
    /// The provider renews ownership while the job still holds the lock handle.
    /// </summary>
    public int? LockLeaseSeconds { get; set; }

    /// <summary>
    /// Specifies the maximum time in seconds to wait for acquiring a lock before timing out.
    /// </summary>
    public int? LockWaitSeconds { get; set; }   // e.g. 60

    /// <summary>
    /// Specifies the interval, in seconds, at which the system will attempt to acquire a lock
    /// while waiting for its availability during job execution.
    /// </summary>
    public int? LockPollSeconds { get; set; }   // e.g. 2
}
