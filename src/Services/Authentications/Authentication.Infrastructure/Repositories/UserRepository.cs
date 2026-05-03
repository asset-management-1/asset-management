namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for users.
/// </summary>
public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly AuthenticationDbContext _authenticationDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public UserRepository(AuthenticationDbContext dbContext) : base(dbContext)
    {
        _authenticationDbContext = dbContext;
    }

    /// <summary>
    /// Gets a user by username for authentication.
    /// </summary>
    /// <param name="normalizedUserName">The normalized username used for login.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user with status, roles, and permissions; otherwise <c>null</c>.</returns>
    public Task<User> GetUserForAuthenticationByUserNameAsync(
        string normalizedUserName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedUserName))
        {
            return Task.FromResult<User>(null);
        }

        // Load user status and authorization data required for login validation and JWT creation.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .Include(x => x.Status)
            .Include(x => x.UserRoles.Where(y => !y.IsDeleted))
                .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RolePermissions.Where(y => !y.IsDeleted))
                        .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(
                x => !x.IsDeleted
                     && x.UserName == normalizedUserName,
                cancellationToken);
    }

    /// <summary>
    /// Gets a user by internal identifier for authentication.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user with status, roles, and permissions; otherwise <c>null</c>.</returns>
    public Task<User> GetUserForAuthenticationByIdAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        // Load user status and authorization data required for refresh-token validation and JWT creation.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .Include(x => x.Status)
            .Include(x => x.UserRoles.Where(y => !y.IsDeleted))
                .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RolePermissions.Where(y => !y.IsDeleted))
                        .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(
                x => !x.IsDeleted
                     && x.Id == userId,
                cancellationToken);
    }

    /// <summary>
    /// Gets a user by public identifier.
    /// </summary>
    /// <param name="userPublicId">The public user identifier used by API and UI.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user; otherwise <c>null</c>.</returns>
    public Task<User> GetByPublicIdAsync(
        Guid userPublicId,
        CancellationToken cancellationToken = default)
    {
        // Resolve public identifier to internal user record without exposing database identity.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => !x.IsDeleted
                     && x.PublicId == userPublicId,
                cancellationToken);
    }

    /// <summary>
    /// Gets a user by normalized email.
    /// </summary>
    /// <param name="normalizedEmail">The normalized email address.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user; otherwise <c>null</c>.</returns>
    public Task<User> GetByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return Task.FromResult<User>(null);
        }

        // Find user by email for account lookup and password reset flows.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => !x.IsDeleted
                     && x.Email == normalizedEmail,
                cancellationToken);
    }

    /// <summary>
    /// Gets detailed user profile information by public identifier.
    /// </summary>
    /// <param name="userPublicId">The public user identifier used by API and UI.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user with party, roles, permissions, and external login links; otherwise <c>null</c>.</returns>
    public Task<User> GetUserInfoByPublicIdAsync(
        Guid userPublicId,
        CancellationToken cancellationToken = default)
    {
        // Load profile, authorization data, and linked external providers for user profile details.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .Include(x => x.Party)
            .Include(x => x.UserRoles.Where(y => !y.IsDeleted))
                .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RolePermissions.Where(y => !y.IsDeleted))
                        .ThenInclude(x => x.Permission)
            .Include(x => x.ExternalLogins.Where(y => !y.IsDeleted))
            .FirstOrDefaultAsync(
                x => !x.IsDeleted
                     && x.PublicId == userPublicId,
                cancellationToken);
    }

    /// <summary>
    /// Checks whether a username already exists.
    /// </summary>
    /// <param name="normalizedUserName">The normalized username to check.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns><c>true</c> if the username exists; otherwise <c>false</c>.</returns>
    public Task<bool> UserNameExistsAsync(
        string normalizedUserName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedUserName))
        {
            return Task.FromResult(false);
        }

        // Prevent duplicate username registration.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .AnyAsync(
                x => !x.IsDeleted
                     && x.UserName == normalizedUserName,
                cancellationToken);
    }

    /// <summary>
    /// Checks whether an email already exists.
    /// </summary>
    /// <param name="normalizedEmail">The normalized email address to check.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns><c>true</c> if the email exists; otherwise <c>false</c>.</returns>
    public Task<bool> EmailExistsAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return Task.FromResult(false);
        }

        // Prevent duplicate email registration.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .AnyAsync(
                x => !x.IsDeleted
                     && x.Email == normalizedEmail,
                cancellationToken);
    }

    /// <summary>
    /// Checks whether a phone number already exists.
    /// </summary>
    /// <param name="phoneNumber">The phone number to check.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns><c>true</c> if the phone number exists; otherwise <c>false</c>.</returns>
    public Task<bool> PhoneNumberExistsAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return Task.FromResult(false);
        }

        // Prevent duplicate phone number registration.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .AnyAsync(
                x => !x.IsDeleted
                     && x.PhoneNumber == phoneNumber,
                cancellationToken);
    }
}