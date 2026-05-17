namespace Be.Haven.Core.Models.ObjectStorage;

/// <summary>
/// Represents the result of best-effort object-storage cleanup.
/// </summary>
public class ObjectStorageCleanupResultModel
{
    /// <summary>
    /// Gets or sets the number of unique object keys submitted for cleanup.
    /// </summary>
    public int AttemptedCount { get; set; }

    /// <summary>
    /// Gets or sets the number of cleanup calls that failed.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Gets a value indicating whether any cleanup call failed.
    /// </summary>
    public bool HasFailures => FailedCount > 0;
}
