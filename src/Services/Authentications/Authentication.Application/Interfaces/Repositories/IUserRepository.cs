using Authentication.Domain.Entities;

namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Defines user-specific repository operations.
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    /// <summary>
    /// Loads a user by normalized username with authentication graph data.
    /// </summary>
    Task<User> GetUserForAuthenticationByUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a user by internal identifier with authentication graph data.
    /// </summary>
    Task<User> GetUserForAuthenticationByIdAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a user by public identifier.
    /// </summary>
    Task<User> GetByPublicIdAsync(Guid userPublicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a user by normalized email address.
    /// </summary>
    Task<User> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a user by public identifier with party, roles, permissions, and external logins.
    /// </summary>
    Task<User> GetUserInfoByPublicIdAsync(Guid userPublicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a username already exists.
    /// </summary>
    Task<bool> UserNameExistsAsync(string normalizedUserName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether an email address already exists.
    /// </summary>
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a phone number already exists.
    /// </summary>
    Task<bool> PhoneNumberExistsAsync(string phoneNumber, CancellationToken cancellationToken = default);
}
