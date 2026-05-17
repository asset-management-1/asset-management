namespace Be.Haven.Cache.Helpers;

/// <summary>
/// Builds lifetimes for epoch-scoped cache correctness metadata.
/// </summary>
internal static class CacheEpochTtlHelper
{
    /// <summary>
    /// Returns the remaining lifetime of the current UTC cache epoch.
    /// </summary>
    /// <returns>A positive lifetime that ends when the current epoch namespace rolls over.</returns>
    public static TimeSpan GetCurrentEpochLifetime()
    {
        var now = DateTimeOffset.UtcNow;
        var nextEpoch = new DateTimeOffset(now.UtcDateTime.Date.AddDays(1), TimeSpan.Zero);
        return nextEpoch - now;
    }
}
