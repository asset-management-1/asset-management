using Authentication.Domain.Entities;

namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Defines refresh-token-specific repository operations.
/// </summary>
public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    /// <summary>
    /// Loads a refresh token together with user graph required for token refresh.
    /// </summary>
    Task<RefreshToken> GetForLoginAsync(string refreshTokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a refresh token owned by a user.
    /// </summary>
    Task<RefreshToken> GetByUserAndHashAsync(long userId, string refreshTokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads all active refresh tokens of a user.
    /// </summary>
    Task<List<RefreshToken>> GetActiveByUserIdAsync(long userId, CancellationToken cancellationToken = default);
}
