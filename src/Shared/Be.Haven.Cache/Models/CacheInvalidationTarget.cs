namespace Be.Haven.Cache.Models;

/// <summary>
/// Represents a logical cache namespace that should be invalidated.
/// </summary>
public sealed class CacheInvalidationTarget : IEquatable<CacheInvalidationTarget>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheInvalidationTarget"/> class.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group name.</param>
    /// <param name="cacheScope">The optional scoped slice within the cache group.</param>
    public CacheInvalidationTarget(string cacheGroup, string cacheScope = "")
    {
        CacheGroup = cacheGroup;
        CacheScope = cacheScope;
    }

    /// <summary>
    /// Gets the logical cache group name.
    /// </summary>
    public string CacheGroup { get; }

    /// <summary>
    /// Gets the optional scoped slice within the cache group.
    /// </summary>
    public string CacheScope { get; }

    /// <summary>
    /// Determines whether the current target equals another cache invalidation target.
    /// </summary>
    /// <param name="other">The other target to compare.</param>
    /// <returns><c>true</c> when both targets share the same group and scope; otherwise <c>false</c>.</returns>
    public bool Equals(CacheInvalidationTarget other)
        => other is not null
           && string.Equals(CacheGroup, other.CacheGroup, StringComparison.Ordinal)
           && string.Equals(CacheScope, other.CacheScope, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current target equals another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><c>true</c> when the object is an equal cache invalidation target; otherwise <c>false</c>.</returns>
    public override bool Equals(object obj) => Equals(obj as CacheInvalidationTarget);

    /// <summary>
    /// Returns the hash code for the current target.
    /// </summary>
    /// <returns>The ordinal hash code for the group and scope pair.</returns>
    public override int GetHashCode() => HashCode.Combine(CacheGroup, CacheScope);
}
