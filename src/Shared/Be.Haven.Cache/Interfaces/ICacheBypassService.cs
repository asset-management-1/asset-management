namespace Be.Haven.Cache.Interfaces;

/// <summary>
/// Tracks cache scopes that should temporarily bypass reads after consistency-critical invalidation failures.
/// </summary>
public interface ICacheBypassService
{
    /// <summary>
    /// Marks one logical cache scope as bypass-only for a short safety window.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group.</param>
    /// <param name="cacheScope">The optional cache scope.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be canceled.</param>
    Task MarkBypassAsync(string cacheGroup, string cacheScope, CancellationToken cancellationToken);

    /// <summary>
    /// Returns whether the logical cache scope should currently bypass cached payloads.
    /// </summary>
    /// <param name="cacheGroup">The logical cache group.</param>
    /// <param name="cacheScope">The optional cache scope.</param>
    /// <returns><c>true</c> when cached payloads should be skipped; otherwise <c>false</c>.</returns>
    /// <param name="cancellationToken">Propagates notification that the operation should be canceled.</param>
    Task<bool> ShouldBypassAsync(string cacheGroup, string cacheScope, CancellationToken cancellationToken);
}
