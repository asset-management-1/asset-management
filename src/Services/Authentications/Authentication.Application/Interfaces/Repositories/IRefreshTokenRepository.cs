namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Defines refresh-token-specific repository operations.
/// </summary>
public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    /// <summary>
    /// Loads and locks the refresh-token row matched by the current or immediately previous token hash.
    /// </summary>
    /// <param name="refreshTokenHash">The hash of the presented refresh token.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked locked refresh-token row; otherwise <c>null</c>.</returns>
    Task<RefreshToken> GetForRefreshForUpdateAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the client-session refresh token for one user and client instance.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="deviceId">The stable client device identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matching client-session row; otherwise <c>null</c>.</returns>
    Task<RefreshToken> GetClientSessionByUserAndDeviceIdAsync(
        long userId,
        string deviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the tracked refresh-token session identified by the authenticated user and session claim.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="sessionPublicId">The server-issued session identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked session row; otherwise <c>null</c>.</returns>
    Task<RefreshToken> GetByUserAndSessionPublicIdAsync(
        long userId,
        Guid sessionPublicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads and locks the current refresh-token session for authenticated password rotation.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="sessionPublicId">The server-issued session identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked locked current-session row; otherwise <c>null</c>.</returns>
    Task<RefreshToken> GetByUserAndSessionPublicIdForUpdateAsync(
        long userId,
        Guid sessionPublicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages one refresh-token revocation without loading or modifying unrelated columns.
    /// </summary>
    /// <param name="refreshTokenId">The internal refresh-token identifier.</param>
    /// <param name="revokedAt">The UTC revocation timestamp.</param>
    /// <param name="replacedByTokenHash">The optional hash of the replacement refresh token.</param>
    /// <returns>A task that completes when the revocation is staged.</returns>
    Task StageRevocationAsync(long refreshTokenId, DateTime revokedAt, string replacedByTokenHash = null);

    /// <summary>
    /// Stages active refresh-token revocations for one user without loading full token rows.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="revokedAt">The UTC revocation timestamp.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>A task that completes when the revocations are staged.</returns>
    Task StageActiveRevocationsByUserIdAsync(long userId, DateTime revokedAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages revocation of every active user session except the current authenticated session.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="currentSessionPublicId">The session identifier that remains active.</param>
    /// <param name="revokedAt">The UTC revocation timestamp.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>A task that completes when the revocations are staged.</returns>
    Task StageActiveRevocationsExceptSessionAsync(
        long userId,
        Guid currentSessionPublicId,
        DateTime revokedAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages active refresh-token revocations for one server-issued session without loading full token rows.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="sessionPublicId">The server-issued session identifier.</param>
    /// <param name="revokedAt">The UTC revocation timestamp.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>A task that completes when the session revocations are staged.</returns>
    Task StageActiveRevocationsByUserAndSessionPublicIdAsync(
        long userId,
        Guid sessionPublicId,
        DateTime revokedAt,
        CancellationToken cancellationToken = default);
}
