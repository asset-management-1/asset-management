namespace Be.Haven.ApiCommon.Interfaces;

/// <summary>
/// Validates server-issued client sessions for Haven access tokens.
/// </summary>
public interface IClientSessionValidator
{
    /// <summary>
    /// Determines whether the supplied session is active and belongs to the supplied user.
    /// </summary>
    /// <param name="userPublicId">The token user's public identifier.</param>
    /// <param name="sessionPublicId">The token session's public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the validation lookup.</param>
    /// <returns><c>true</c> when the session is active for the user; otherwise <c>false</c>.</returns>
    Task<bool> IsSessionActiveAsync(
        Guid userPublicId,
        Guid sessionPublicId,
        CancellationToken cancellationToken = default);
}
