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
