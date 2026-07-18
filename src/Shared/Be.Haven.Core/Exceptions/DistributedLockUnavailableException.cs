namespace Be.Haven.Core.Exceptions;

/// <summary>
/// Represents an infrastructure failure that prevents the distributed lock provider from coordinating a mutation.
/// </summary>
public sealed class DistributedLockUnavailableException : Exception
{
    /// <summary>
    /// Initializes a new exception while preserving the provider failure for server-side diagnostics.
    /// </summary>
    /// <param name="innerException">The underlying provider or connection failure.</param>
    public DistributedLockUnavailableException(Exception innerException)
        : base(ErrorConstants.DistributedLockErrors.DISTRIBUTED_LOCK_UNAVAILABLE, innerException)
    {
    }
}
