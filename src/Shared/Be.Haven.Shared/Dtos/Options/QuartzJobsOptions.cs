namespace Be.Haven.Shared.Dtos.Options;

public class QuartzJobsOptions
{
    /// <summary>
    /// Gets or sets the prefix used for naming Quartz job-related resources, such as lock keys.
    /// </summary>
    public string ServicePrefix { get; set; } = "na";

    /// <summary>
    /// Gets or sets the collection of Quartz job configurations, where each job is identified by a unique key.
    /// </summary>
    public Dictionary<string, QuartzJobConfigOptions> Jobs { get; set; } = new();
}