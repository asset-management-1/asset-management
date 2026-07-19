namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Defines user-specific repository operations.
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    /// <summary>
    /// Loads a user by normalised username with authentication graph data.
    /// </summary>
    /// <param name="normalizedUserName">The normalised username used for login.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user with authentication state; otherwise <c>null</c>.</returns>
    Task<User> GetUserForAuthenticationByUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a user by internal identifier with authentication graph data.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user with authentication state; otherwise <c>null</c>.</returns>
    Task<User> GetUserForAuthenticationByIdAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a user by public identifier.
    /// </summary>
    /// <param name="userPublicId">The public user identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user bridge record; otherwise <c>null</c>.</returns>
    Task<User> GetByPublicIdAsync(Guid userPublicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a tracked user by public identifier for update flows.
    /// </summary>
    /// <param name="userPublicId">The public user identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked user entity; otherwise <c>null</c>.</returns>
    Task<User> GetTrackedByPublicIdAsync(Guid userPublicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the minimal user fields required for password validation by public identifier.
    /// </summary>
    /// <param name="userPublicId">The public user identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched password identity; otherwise <c>null</c>.</returns>
    Task<User> GetPasswordIdentityByPublicIdAsync(Guid userPublicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a user by normalised email address.
    /// </summary>
    /// <param name="normalizedEmail">The normalised email address.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user; otherwise <c>null</c>.</returns>
    Task<User> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the minimal user fields required for password reset by normalised email address.
    /// </summary>
    /// <param name="normalizedEmail">The normalised email address.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched password identity; otherwise <c>null</c>.</returns>
    Task<User> GetPasswordIdentityByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads and locks the user row identified by normalised email for password reset mutation.
    /// </summary>
    /// <param name="normalizedEmail">The normalised email authorised by the consumed reset session.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked locked password identity; otherwise <c>null</c>.</returns>
    Task<User> GetPasswordIdentityByEmailForUpdateAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a last-login timestamp update without loading the full user entity.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="lastLoginAt">The UTC last-login timestamp.</param>
    /// <returns>A task that completes when the update is staged.</returns>
    Task StageLastLoginAtAsync(long userId, DateTime lastLoginAt);

    /// <summary>
    /// Stages a password-hash update without loading the full user entity.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="passwordHash">The new password hash.</param>
    /// <param name="authResetAt">The UTC authentication reset marker.</param>
    /// <param name="updatedAt">The UTC update timestamp.</param>
    /// <returns>A task that completes when the update is staged.</returns>
    Task StagePasswordHashChangeAsync(
        long userId,
        string passwordHash,
        DateTime authResetAt,
        DateTime updatedAt);

    /// <summary>
    /// Stages an authentication reset timestamp without loading the full user entity.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="authResetAt">The UTC authentication reset marker.</param>
    /// <param name="updatedAt">The UTC update timestamp.</param>
    /// <returns>A task that completes when the update is staged.</returns>
    Task StageAuthResetAsync(long userId, DateTime authResetAt, DateTime updatedAt);

    /// <summary>
    /// Loads a user by public identifier together with active external-login mappings.
    /// </summary>
    /// <param name="userPublicId">The public user identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user with active external logins; otherwise <c>null</c>.</returns>
    Task<User> GetByPublicIdWithExternalLoginsAsync(Guid userPublicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the user-info response model by public identifier using an optimised read query.
    /// </summary>
    /// <param name="userPublicId">The public identifier of the user to load.</param>
    /// <param name="sessionPublicId">The authenticated client-session identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The assembled user-info response, or <c>null</c> when the user is not found.</returns>
    Task<UserInfoResponseDto> GetUserInfoResponseByPublicIdAsync(
        Guid userPublicId,
        Guid sessionPublicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads and locks one active user row for a security-sensitive mutation.
    /// </summary>
    /// <param name="userPublicId">The public user identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked locked user; otherwise <c>null</c>.</returns>
    Task<User> GetTrackedByPublicIdForUpdateAsync(
        Guid userPublicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a username already exists.
    /// </summary>
    /// <param name="normalizedUserName">The normalised username.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns><c>true</c> when the username exists; otherwise <c>false</c>.</returns>
    Task<bool> UserNameExistsAsync(string normalizedUserName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether an email address already exists.
    /// </summary>
    /// <param name="normalizedEmail">The normalised email address.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns><c>true</c> when the email exists; otherwise <c>false</c>.</returns>
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a phone number already exists.
    /// </summary>
    /// <param name="phoneNumber">The phone number to check.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns><c>true</c> when the phone number exists; otherwise <c>false</c>.</returns>
    Task<bool> PhoneNumberExistsAsync(string phoneNumber, CancellationToken cancellationToken = default);
}
