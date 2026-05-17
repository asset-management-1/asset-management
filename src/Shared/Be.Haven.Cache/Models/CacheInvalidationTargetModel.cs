namespace Be.Haven.Cache.Models;

/// <summary>
/// Represents a logical cache namespace that should be invalidated.
/// </summary>
public sealed class CacheInvalidationTargetModel : IEquatable<CacheInvalidationTargetModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheInvalidationTargetModel"/> class.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group name.</param>
    /// <param name="cacheScope">The optional scoped slice within the cache group.</param>
    /// <param name="failOnError">Whether invalidation failure should fail the command pipeline.</param>
    public CacheInvalidationTargetModel(
        string cacheGroup,
        string cacheScope = "",
        bool failOnError = false)
    {
        CacheGroup = cacheGroup;
        CacheScope = cacheScope;
        FailOnError = failOnError;
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
    /// Gets a value indicating whether invalidation failure should fail the command pipeline.
    /// </summary>
    public bool FailOnError { get; }

    /// <summary>
    /// Determines whether the current target equals another cache invalidation target.
    /// </summary>
    /// <param name="other">The other target to compare.</param>
    /// <returns><c>true</c> when both targets share the same group, scope, and failure policy; otherwise <c>false</c>.</returns>
    public bool Equals(CacheInvalidationTargetModel other)
        => other is not null
           && string.Equals(CacheGroup, other.CacheGroup, StringComparison.Ordinal)
           && string.Equals(CacheScope, other.CacheScope, StringComparison.Ordinal)
           && FailOnError == other.FailOnError;

    /// <summary>
    /// Determines whether the current target equals another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><c>true</c> when the object is an equal cache invalidation target; otherwise <c>false</c>.</returns>
    public override bool Equals(object obj) => Equals(obj as CacheInvalidationTargetModel);

    /// <summary>
    /// Returns the hash code for the current target.
    /// </summary>
    /// <returns>The ordinal hash code for the group, scope, and failure policy.</returns>
    public override int GetHashCode() => HashCode.Combine(CacheGroup, CacheScope, FailOnError);
}

