namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for users.
/// </summary>
public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly AuthenticationDbContext _authenticationDbContext;
    private readonly IDapperService _dapperService;
    private readonly IJsonSerializerService _jsonSerializerService;

    /// <summary>
    /// Creates the user repository with EF write access and optimised Dapper read models.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    /// <param name="dapperService">The Dapper service used for optimised read queries.</param>
    /// <param name="jsonSerializerService">The shared JSON serializer used for SQL JSON projections.</param>
    public UserRepository(
        AuthenticationDbContext dbContext,
        IDapperService dapperService,
        IJsonSerializerService jsonSerializerService) : base(dbContext)
    {
        _authenticationDbContext = dbContext;
        _dapperService = dapperService;
        _jsonSerializerService = jsonSerializerService;
    }

    /// <summary>
    /// Gets a user by username for authentication.
    /// </summary>
    /// <param name="normalizedUserName">The normalised username used for login.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user with the status required for authentication; otherwise <c>null</c>.</returns>
    public Task<User> GetUserForAuthenticationByUserNameAsync(
        string normalizedUserName,
        CancellationToken cancellationToken = default)
    {
        // Empty credentials cannot match an account, so avoid an unnecessary database round-trip.
        if (string.IsNullOrWhiteSpace(normalizedUserName))
        {
            return Task.FromResult<User>(null);
        }

        // Load only the fields needed to validate login and issue tokens.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .Where(
                x => !x.IsDeleted
                     && x.UserName == normalizedUserName)
            .Select(x => new User
            {
                Id = x.Id,
                PublicId = x.PublicId,
                UserName = x.UserName,
                Email = x.Email,
                AuthResetAt = x.AuthResetAt,
                PasswordHash = x.PasswordHash,
                EmailConfirmed = x.EmailConfirmed,
                IsDeleted = x.IsDeleted,
                Status = new MasterDataValue
                {
                    Id = x.Status.Id,
                    Code = x.Status.Code,
                    Name = x.Status.Name
                }
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a user by internal identifier for authentication.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user with the status required for authentication; otherwise <c>null</c>.</returns>
    public Task<User> GetUserForAuthenticationByIdAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        // Load the minimal authentication shape used after refresh-token lookup.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .Where(
                x => !x.IsDeleted
                     && x.Id == userId)
            .Select(x => new User
            {
                Id = x.Id,
                PublicId = x.PublicId,
                UserName = x.UserName,
                Email = x.Email,
                AuthResetAt = x.AuthResetAt,
                PasswordHash = x.PasswordHash,
                EmailConfirmed = x.EmailConfirmed,
                IsDeleted = x.IsDeleted,
                Status = new MasterDataValue
                {
                    Id = x.Status.Id,
                    Code = x.Status.Code,
                    Name = x.Status.Name
                }
            })
            .FirstOrDefaultAsync(cancellationToken);
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
        // Public-id lookups only need the internal id bridge for downstream writes.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.PublicId == userPublicId)
            .Select(x => new User
            {
                Id = x.Id,
                PublicId = x.PublicId,
                AuthResetAt = x.AuthResetAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a tracked user by public identifier for update flows.
    /// </summary>
    /// <param name="userPublicId">The public user identifier used by API and UI.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked matched user; otherwise <c>null</c>.</returns>
    public Task<User> GetTrackedByPublicIdAsync(
        Guid userPublicId,
        CancellationToken cancellationToken = default)
    {
        // Return a tracked entity because callers mutate profile or account fields.
        return _authenticationDbContext.Users
            .FirstOrDefaultAsync(
                x => !x.IsDeleted
                     && x.PublicId == userPublicId,
                cancellationToken);
    }

    /// <summary>
    /// Gets the minimal password identity by public identifier.
    /// </summary>
    /// <param name="userPublicId">The public user identifier used by API and UI.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched password identity; otherwise <c>null</c>.</returns>
    public Task<User> GetPasswordIdentityByPublicIdAsync(
        Guid userPublicId,
        CancellationToken cancellationToken = default)
    {
        // Project only password fields required by authenticated password changes.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.PublicId == userPublicId)
            .Select(x => new User
            {
                Id = x.Id,
                PublicId = x.PublicId,
                UserName = x.UserName,
                AuthResetAt = x.AuthResetAt,
                PasswordHash = x.PasswordHash
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a user by normalised email.
    /// </summary>
    /// <param name="normalizedEmail">The normalised email address.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user; otherwise <c>null</c>.</returns>
    public Task<User> GetByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        // Blank email checks are treated as no match and do not hit the database.
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return Task.FromResult<User>(null);
        }

        // Keep the projection minimal because this path is used only for existence/conflict checks.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Email == normalizedEmail)
            .Select(x => new User
            {
                Id = x.Id,
                Email = x.Email
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the minimal password identity by normalised email.
    /// </summary>
    /// <param name="normalizedEmail">The normalised email address.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched password identity; otherwise <c>null</c>.</returns>
    public Task<User> GetPasswordIdentityByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        // Blank reset identifiers cannot resolve to a password identity.
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return Task.FromResult<User>(null);
        }

        // Project the password identity needed by forgot-password completion only.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Email == normalizedEmail)
            .Select(x => new User
            {
                Id = x.Id,
                PublicId = x.PublicId,
                UserName = x.UserName,
                AuthResetAt = x.AuthResetAt,
                PasswordHash = x.PasswordHash
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Stages a last-login timestamp update on the user row only.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="lastLoginAt">The UTC last-login timestamp.</param>
    /// <returns>A completed task after the field-level update is staged.</returns>
    public Task StageLastLoginAtAsync(
        long userId,
        DateTime lastLoginAt)
    {
        // Attach a stub entity so EF updates only the last-login column.
        var user = new User { Id = userId, LastLoginAt = lastLoginAt };
        _authenticationDbContext.Users.Attach(user);
        _authenticationDbContext.Entry(user).Property(x => x.LastLoginAt).IsModified = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stages a password-hash update on the user row only.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="passwordHash">The new password hash.</param>
    /// <param name="authResetAt">The UTC auth reset marker that invalidates older access tokens.</param>
    /// <param name="updatedAt">The UTC timestamp used for explicit password-change state.</param>
    /// <returns>A completed task after the field-level update is staged.</returns>
    public Task StagePasswordHashChangeAsync(
        long userId,
        string passwordHash,
        DateTime authResetAt,
        DateTime updatedAt)
    {
        // Reuse a row-lock query's tracked entity; attaching a second instance with the same key is invalid in EF.
        var user = _authenticationDbContext.Users.Local.FirstOrDefault(x => x.Id == userId);
        if (user is null)
        {
            user = new User { Id = userId };
            _authenticationDbContext.Users.Attach(user);
        }

        user.PasswordHash = passwordHash;
        user.AuthResetAt = authResetAt;
        user.UpdatedAt = updatedAt;
        _authenticationDbContext.Entry(user).Property(x => x.PasswordHash).IsModified = true;
        _authenticationDbContext.Entry(user).Property(x => x.AuthResetAt).IsModified = true;
        _authenticationDbContext.Entry(user).Property(x => x.UpdatedAt).IsModified = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stages an auth reset timestamp update on the user row only.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="authResetAt">The UTC auth reset marker that invalidates older access tokens.</param>
    /// <param name="updatedAt">The UTC timestamp used for explicit auth-reset state.</param>
    /// <returns>A completed task after the field-level update is staged.</returns>
    public Task StageAuthResetAsync(
        long userId,
        DateTime authResetAt,
        DateTime updatedAt)
    {
        // Attach a stub entity so credential reset flows can invalidate auth state without loading the full row.
        var user = new User
        {
            Id = userId,
            AuthResetAt = authResetAt,
            UpdatedAt = updatedAt
        };

        _authenticationDbContext.Users.Attach(user);
        _authenticationDbContext.Entry(user).Property(x => x.AuthResetAt).IsModified = true;
        _authenticationDbContext.Entry(user).Property(x => x.UpdatedAt).IsModified = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a user by public identifier together with active external-logins.
    /// </summary>
    /// <param name="userPublicId">The public user identifier used by API and UI.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched user with active external-logins; otherwise <c>null</c>.</returns>
    public Task<User> GetByPublicIdWithExternalLoginsAsync(
        Guid userPublicId,
        CancellationToken cancellationToken = default)
    {
        // Load active external-login count data for final sign-in-method guard checks.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.PublicId == userPublicId)
            .Select(x => new User
            {
                Id = x.Id,
                PublicId = x.PublicId,
                PasswordHash = x.PasswordHash,
                ExternalLogins = x.ExternalLogins
                    .Where(y => !y.IsDeleted)
                    .Select(y => new ExternalLogin
                    {
                        Id = y.Id,
                        UserId = y.UserId,
                        LoginProvider = y.LoginProvider
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the user-info response model by public identifier using Dapper.
    /// </summary>
    /// <param name="userPublicId">The public user identifier used by API and UI.</param>
    /// <param name="sessionPublicId">The current refresh-token session identifier used to resolve Party context.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The assembled user-info response; otherwise <c>null</c>.</returns>
    public async Task<UserInfoResponseDto> GetUserInfoResponseByPublicIdAsync(
        Guid userPublicId,
        Guid sessionPublicId,
        CancellationToken cancellationToken = default)
    {
        // Use the optimised PostgreSQL read model query because user-info assembles several child collections.
        var readModel = await _dapperService.QueryFirstOrDefaultAsync<UserInfoReadModel>(
            InfrastructureQueryConstants.GET_USER_INFO_RESPONSE_BY_PUBLIC_ID_QUERY,
            new { UserPublicId = userPublicId, SessionPublicId = sessionPublicId },
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return MapUserInfoResponse(readModel);
    }

    /// <summary>
    /// Loads and locks one active user for a security-sensitive mutation.
    /// </summary>
    /// <param name="userPublicId">The public identifier of the user to lock.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked user when found; otherwise, <c>null</c>.</returns>
    public async Task<User> GetTrackedByPublicIdForUpdateAsync(
        Guid userPublicId,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Lock the matching user identifier through Dapper on the required EF mutation transaction.
        var userId = await _dapperService.ExecuteScalarAsync<long?>(
            InfrastructureQueryConstants.GET_USER_ID_BY_PUBLIC_ID_FOR_UPDATE_QUERY,
            new { UserPublicId = userPublicId },
            DapperCommandOptionsHelper.CreateTransactionalText(
                _authenticationDbContext,
                cancellationToken));
        if (!userId.HasValue)
        {
            return null;
        }

        // Step 3: Materialise the locked user through EF so the owning mutation receives tracked state.
        return await _authenticationDbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId.Value, cancellationToken);
    }

    /// <summary>
    /// Loads and locks one non-deleted user row for forgot-password completion.
    /// </summary>
    /// <param name="normalizedEmail">The normalised email authorised by the consumed reset session.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked locked password identity when found; otherwise <c>null</c>.</returns>
    public async Task<User> GetPasswordIdentityByEmailForUpdateAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        // Empty reset-session identity cannot select a password owner and must not reach SQL.
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return null;
        }

        // Step 1: Lock the password-owner identifier through Dapper on the required password-change transaction.
        var userId = await _dapperService.ExecuteScalarAsync<long?>(
            InfrastructureQueryConstants.GET_PASSWORD_IDENTITY_USER_ID_BY_EMAIL_FOR_UPDATE_QUERY,
            new { NormalizedEmail = normalizedEmail },
            DapperCommandOptionsHelper.CreateTransactionalText(
                _authenticationDbContext,
                cancellationToken));
        if (!userId.HasValue)
        {
            return null;
        }

        // Step 3: Load the locked identity through EF so password mutation reuses one tracked user row.
        return await _authenticationDbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId.Value, cancellationToken);
    }

    /// <summary>
    /// Maps the optimised single-row Dapper projection into the public user-info response.
    /// </summary>
    /// <param name="readModel">The read model returned by the user-info SQL query.</param>
    /// <returns>The assembled user-info response; otherwise <c>null</c> when no user was found.</returns>
    private UserInfoResponseDto MapUserInfoResponse(UserInfoReadModel readModel)
    {
        // A missing Dapper row means the authenticated user no longer resolves to an active profile.
        if (readModel is null)
        {
            return null;
        }

        // PostgreSQL emits child collections as JSON so the query returns one compact user row.
        var availableContexts = _jsonSerializerService.DeserializeList<string>(readModel.AvailableContextsJson);
        var currentContext = EnumConvertHelper<PartyTypeEnum>.TryConvertStringToEnum(readModel.CurrentContext)
                             ?? throw new InvalidOperationException(
                                 string.Format(
                                     InfrastructureErrorConstants.PartyContextErrors.UNSUPPORTED_PARTY_CONTEXT_VALUE_MESSAGE,
                                     readModel.CurrentContext));

        return new UserInfoResponseDto
        {
            FullName = readModel.FullName,
            UserName = readModel.UserName,
            Email = readModel.Email,
            PhoneNumber = readModel.PhoneNumber,
            AvatarUrl = readModel.AvatarUrl,
            DateOfBirth = readModel.DateOfBirth,
            Gender = EnumConvertHelper<GenderEnum>.TryConvertStringToEnum(readModel.Gender),
            DisplayName = readModel.DisplayName,
            CurrentContext = currentContext,
            AvailableContexts = availableContexts
                .Select(EnumConvertHelper<PartyTypeEnum>.TryConvertStringToEnum)
                .Where(context => context.HasValue)
                .Select(context => context.Value)
                .ToList(),
            ExternalProviders = _jsonSerializerService.DeserializeList<ExternalProviderResponseDto>(readModel.ExternalProvidersJson),
            KycSummary = new KycSummaryResponseDto
            {
                IsSubmitted = readModel.KycIsSubmitted,
                Status = EnumConvertHelper<KycStatusEnum>.TryConvertStringToEnum(readModel.KycStatus),
                IdentifierType = EnumConvertHelper<IdentifierTypeEnum>.TryConvertStringToEnum(readModel.KycIdentifierType),
                IdentifierTypeDisplayName = readModel.KycIdentifierTypeDisplayName,
                MaskedIdentifier = readModel.KycMaskedIdentifier,
                HasFrontFile = readModel.KycHasFrontFile,
                HasBackFile = readModel.KycHasBackFile
            }
        };
    }

    /// <summary>
    /// Checks whether a username already exists.
    /// </summary>
    /// <param name="normalizedUserName">The normalised username to check.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns><c>true</c> if the username exists; otherwise <c>false</c>.</returns>
    public Task<bool> UserNameExistsAsync(
        string normalizedUserName,
        CancellationToken cancellationToken = default)
    {
        // Blank usernames are handled by validators and cannot conflict in persistence.
        if (string.IsNullOrWhiteSpace(normalizedUserName))
        {
            return Task.FromResult(false);
        }

        // Use an existence query instead of loading the matching user row.
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
    /// <param name="normalizedEmail">The normalised email address to check.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns><c>true</c> if the email exists; otherwise <c>false</c>.</returns>
    public Task<bool> EmailExistsAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        // Blank emails are handled by validators and cannot conflict in persistence.
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return Task.FromResult(false);
        }

        // Use an existence query instead of loading the matching user row.
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
        // Empty optional phone numbers do not participate in uniqueness checks.
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return Task.FromResult(false);
        }

        // Use an existence query instead of loading the matching user row.
        return _authenticationDbContext.Users
            .AsNoTracking()
            .AnyAsync(
                x => !x.IsDeleted
                     && x.PhoneNumber == phoneNumber,
                cancellationToken);
    }
}
