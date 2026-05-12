namespace Be.Haven.ApiCommon.Interfaces;

/// <summary>
/// Validates whether a Haven access token was issued after the user's latest authentication reset.
/// </summary>
public interface IAuthResetValidator
{
    /// <summary>
    /// Validates one access token issue timestamp against the current user auth reset marker.
    /// </summary>
    /// <param name="userPublicId">The public user identifier from the validated token.</param>
    /// <param name="issuedAtMs">The token issue timestamp in Unix milliseconds.</param>
    /// <param name="cancellationToken">The token used to cancel cache or database operations.</param>
    /// <returns><c>true</c> when the token remains valid; otherwise <c>false</c>.</returns>
    Task<bool> IsTokenValidAsync(
        Guid userPublicId,
        long issuedAtMs,
        CancellationToken cancellationToken = default);
}
