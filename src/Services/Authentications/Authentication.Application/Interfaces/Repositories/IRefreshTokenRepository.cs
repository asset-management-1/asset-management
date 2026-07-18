namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Defines refresh-token-specific repository operations.
/// </summary>
public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    /// <summary>
    /// Loads a refresh token row together with user graph required for token refresh.
    /// </summary>
    Task<RefreshToken> GetForRefreshAsync(string refreshTokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the client-session refresh token for one user and client instance.
    /// </summary>
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
    /// Stages one refresh-token revocation without loading or modifying unrelated columns.
    /// </summary>
    Task StageRevocationAsync(long refreshTokenId, DateTime revokedAt, string replacedByTokenHash = null);

    /// <summary>
    /// Stages active refresh-token revocations for one user without loading full token rows.
    /// </summary>
    Task StageActiveRevocationsByUserIdAsync(long userId, DateTime revokedAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages active refresh-token revocations for one server-issued session without loading full token rows.
    /// </summary>
    Task StageActiveRevocationsByUserAndSessionPublicIdAsync(
        long userId,
        Guid sessionPublicId,
        DateTime revokedAt,
        CancellationToken cancellationToken = default);
}
