namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Defines external-login-specific repository operations.
/// </summary>
public interface IExternalLoginRepository : IGenericRepository<ExternalLogin>
{
    /// <summary>
    /// Loads an external login mapping by provider and provider user id.
    /// </summary>
    Task<ExternalLogin> GetByProviderAsync(string provider, string providerUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an external login mapping by user and provider.
    /// </summary>
    Task<ExternalLogin> GetByUserAndProviderAsync(long userId, string provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a soft-deleted external login mapping by user and provider.
    /// </summary>
    Task<ExternalLogin> GetDeletedByUserAndProviderAsync(long userId, string provider, CancellationToken cancellationToken = default);
}
