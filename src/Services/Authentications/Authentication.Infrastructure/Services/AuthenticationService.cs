namespace Authentication.Infrastructure.Services;

/// <summary>
/// Implements authentication workflows for local credentials, OTP verification,
/// password recovery, external identity providers, and user profile retrieval.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IExternalLoginRepository _externalLoginRepository;
    private readonly IMasterDataValueRepository _masterDataValueRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPartyRepository _partyRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly ICachingService _cachingService;
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly AuthOptions _authOptions;
    private readonly ExternalAuthenticationOptions _externalAuthenticationOptions;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="refreshTokenRepository">The refresh token repository.</param>
    /// <param name="externalLoginRepository">The external login repository.</param>
    /// <param name="masterDataValueRepository">The master data value repository.</param>
    /// <param name="roleRepository">The role repository.</param>
    /// <param name="partyRepository">The party repository.</param>
    /// <param name="userRoleRepository">The user role repository.</param>
    /// <param name="cachingService">The shared caching service backed by Redis.</param>
    /// <param name="authService">The current request authentication accessor.</param>
    /// <param name="unitOfWork">The unit of work for transactional persistence.</param>
    /// <param name="emailService">The shared email service.</param>
    /// <param name="passwordHasher">The Microsoft password hasher for local credentials.</param>
    /// <param name="authOptions">The JWT settings.</param>
    /// <param name="externalAuthenticationOptions">The external provider settings.</param>
    /// <param name="logger">The logger.</param>
    public AuthenticationService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IExternalLoginRepository externalLoginRepository,
        IMasterDataValueRepository masterDataValueRepository,
        IRoleRepository roleRepository,
        IPartyRepository partyRepository,
        IUserRoleRepository userRoleRepository,
        ICachingService cachingService,
        IAuthService authService,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IPasswordHasher<User> passwordHasher,
        IOptions<AuthOptions> authOptions,
        IOptions<ExternalAuthenticationOptions> externalAuthenticationOptions,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _externalLoginRepository = externalLoginRepository;
        _masterDataValueRepository = masterDataValueRepository;
        _roleRepository = roleRepository;
        _partyRepository = partyRepository;
        _userRoleRepository = userRoleRepository;
        _cachingService = cachingService;
        _authService = authService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _authOptions = authOptions.Value ?? new AuthOptions();
        _externalAuthenticationOptions = externalAuthenticationOptions.Value ?? new ExternalAuthenticationOptions();
    }

    /// <summary>
    /// Authenticates a user with local credentials and issues an access token with a refresh token.
    /// </summary>
    public async Task<ResponseDto<LoginResponse>> LoginAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = NormalizeUserName(userName);
        if (string.IsNullOrWhiteSpace(normalizedUserName) || string.IsNullOrWhiteSpace(password))
        {
            return new ResponseDto<LoginResponse>(AUTH_INVALID_CREDENTIALS, INVALID_USERNAME_OR_PASSWORD_MESSAGE);
        }

        var user = await _userRepository.GetUserForAuthenticationByUserNameAsync(normalizedUserName, cancellationToken);
        if (user is null || !CanLogin(user))
        {
            return new ResponseDto<LoginResponse>(AUTH_INVALID_CREDENTIALS, INVALID_USERNAME_OR_PASSWORD_MESSAGE);
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return new ResponseDto<LoginResponse>(AUTH_INVALID_CREDENTIALS, INVALID_USERNAME_OR_PASSWORD_MESSAGE);
        }

        user.LastLoginAt = DateTime.UtcNow;
        return await CreateLoginResponseAsync(user, cancellationToken);
    }

    /// <summary>
    /// Starts the register flow by validating uniqueness, storing pending data in Redis, and sending an OTP email.
    /// </summary>
    public async Task<ResponseDto<string>> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(command.Email);
        var normalizedUserName = NormalizeUserName(command.UserName);

        var uniquenessError = await CheckRegistrationUniquenessAsync(normalizedUserName, normalizedEmail, command.PhoneNumber, cancellationToken);
        if (uniquenessError is not null)
        {
            return uniquenessError;
        }

        var partyType = await FindMasterDataValueAsync(PARTY_TYPE_TYPE, command.PartyType, cancellationToken);
        if (partyType is null)
        {
            return new ResponseDto<string>(AUTH_FORBIDDEN_OPERATION, INVALID_PARTY_TYPE_MESSAGE);
        }

        var throttleError = await CheckOtpThrottleAsync(REGISTER_PURPOSE, normalizedEmail, REGISTER_OTP_LIMIT, cancellationToken);
        if (throttleError is not null)
        {
            return throttleError;
        }

        var passwordHash = _passwordHasher.HashPassword(
            new User { UserName = normalizedUserName, Email = normalizedEmail },
            command.Password);

        var pendingRegister = new PendingRegisterCacheEntry
        {
            UserName = normalizedUserName,
            Email = normalizedEmail,
            PhoneNumber = command.PhoneNumber.Trim(),
            FullName = command.FullName.Trim(),
            PartyType = command.PartyType.Trim(),
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        var otpCode = GenerateOtp();
        await SetOtpAsync(REGISTER_PURPOSE, normalizedEmail, otpCode, cancellationToken);
        await SetCacheAsync(
            GetPendingRegisterKey(normalizedEmail),
            pendingRegister,
            TimeSpan.FromMinutes(OTP_TTL_MINUTES),
            cancellationToken);

        var sent = await SendOtpEmailAsync(normalizedEmail, otpCode, EMAIL_SUBJECT_VERIFY_ACCOUNT, REGISTER_PURPOSE, cancellationToken);
        if (!sent)
        {
            await RemoveCacheAsync(GetOtpKey(REGISTER_PURPOSE, normalizedEmail), cancellationToken);
            await RemoveCacheAsync(GetPendingRegisterKey(normalizedEmail), cancellationToken);
            return new ResponseDto<string>(AUTH_FORBIDDEN_OPERATION, OTP_SEND_FAILED_MESSAGE);
        }

        await SetCooldownAsync(REGISTER_PURPOSE, normalizedEmail, cancellationToken);
        _logger.LogInformation(LOG_REGISTER_OTP_SENT);
        return new ResponseDto<string>(OTP_SENT_MESSAGE);
    }

    /// <summary>
    /// Completes registration after the email OTP is verified successfully.
    /// </summary>
    public async Task<ResponseDto<string>> VerifyRegisterEmailAsync(VerifyRegisterEmailCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(command.Email);
        var otpVerification = await VerifyOtpAsync(REGISTER_PURPOSE, normalizedEmail, command.Otp, cancellationToken);
        if (!otpVerification.Success)
        {
            return otpVerification;
        }

        var pendingRegister = await GetCacheAsync<PendingRegisterCacheEntry>(GetPendingRegisterKey(normalizedEmail), cancellationToken);
        if (pendingRegister is null)
        {
            return new ResponseDto<string>(AUTH_PENDING_REGISTER_NOT_FOUND, REGISTRATION_SESSION_EXPIRED_MESSAGE);
        }

        var uniquenessError = await CheckRegistrationUniquenessAsync(
            pendingRegister.UserName,
            pendingRegister.Email,
            pendingRegister.PhoneNumber,
            cancellationToken);
        if (uniquenessError is not null)
        {
            return uniquenessError;
        }

        var partyType = await FindMasterDataValueAsync(PARTY_TYPE_TYPE, pendingRegister.PartyType, cancellationToken);
        var activeStatus = await FindMasterDataValueAsync(STATUS_TYPE, ACTIVE_STATUS, cancellationToken);
        if (partyType is null || activeStatus is null)
        {
            return new ResponseDto<string>(AUTH_USER_STATUS_NOT_FOUND, REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE);
        }

        var role = await FindRoleForPartyTypeAsync(pendingRegister.PartyType, cancellationToken);
        if (role is null)
        {
            return new ResponseDto<string>(AUTH_FORBIDDEN_OPERATION, DEFAULT_ROLE_NOT_FOUND_MESSAGE);
        }

        var party = new Party
        {
            PartyTypeId = partyType.Id,
            DisplayName = pendingRegister.FullName,
            LegalName = pendingRegister.FullName,
            PrimaryEmail = pendingRegister.Email,
            PrimaryPhone = pendingRegister.PhoneNumber,
            StatusId = activeStatus.Id
        };

        var user = new User
        {
            UserName = pendingRegister.UserName,
            Email = pendingRegister.Email,
            PhoneNumber = pendingRegister.PhoneNumber,
            PasswordHash = pendingRegister.PasswordHash,
            EmailConfirmed = true,
            FullName = pendingRegister.FullName,
            StatusId = activeStatus.Id,
            LockoutEnabled = true
        };

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _partyRepository.AddAsync(party, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            user.PartyId = party.Id;

            await _userRepository.AddAsync(user, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            await _userRoleRepository.AddAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            }, ct);
        }, cancellationToken);

        await RemoveCacheAsync(GetPendingRegisterKey(normalizedEmail), cancellationToken);
        await RemoveCacheAsync(GetOtpKey(REGISTER_PURPOSE, normalizedEmail), cancellationToken);

        _logger.LogInformation(LOG_REGISTER_COMPLETED, user.PublicId);
        return new ResponseDto<string>(REGISTRATION_COMPLETED_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Re-issues an access token and rotates the refresh token when the supplied refresh token is valid.
    /// </summary>
    public async Task<ResponseDto<LoginResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return new ResponseDto<LoginResponse>(AUTH_INVALID_REFRESH_TOKEN, INVALID_REFRESH_TOKEN_MESSAGE);
        }

        var refreshTokenHash = HashToken(refreshToken);
        var existingToken = await _refreshTokenRepository.GetForLoginAsync(refreshTokenHash, cancellationToken);

        if (existingToken is null || existingToken.RevokedAt.HasValue || existingToken.ExpiresAt <= DateTime.UtcNow || !CanLogin(existingToken.User))
        {
            return new ResponseDto<LoginResponse>(AUTH_INVALID_REFRESH_TOKEN, INVALID_REFRESH_TOKEN_MESSAGE);
        }

        var rawRefreshToken = GenerateRefreshToken();
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByTokenHash = HashToken(rawRefreshToken);

        return new ResponseDto<LoginResponse>(await BuildLoginResponseAsync(existingToken.User, rawRefreshToken, cancellationToken));
    }

    /// <summary>
    /// Revokes either one refresh token or all refresh tokens of the specified user.
    /// </summary>
    public async Task<ResponseDto<string>> LogoutAsync(Guid userPublicId, string refreshToken, bool logoutAllSessions, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByPublicIdAsync(userPublicId, cancellationToken);
        if (user is null)
        {
            return new ResponseDto<string>(AUTH_UNAUTHORIZED, UNAUTHORIZED_REQUEST_MESSAGE);
        }

        if (logoutAllSessions)
        {
            await RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
            _logger.LogInformation(LOG_LOGOUT_ALL_REVOKED, user.PublicId);
            return new ResponseDto<string>(LOGOUT_SUCCESS_MESSAGE);
        }

        var refreshTokenHash = HashToken(refreshToken);
        var existingToken = await _refreshTokenRepository.GetByUserAndHashAsync(user.Id, refreshTokenHash, cancellationToken);

        if (existingToken is null)
        {
            return new ResponseDto<string>(AUTH_INVALID_REFRESH_TOKEN, INVALID_REFRESH_TOKEN_MESSAGE);
        }

        existingToken.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(LOG_LOGOUT_REFRESH_TOKEN_REVOKED, user.PublicId);
        return new ResponseDto<string>(LOGOUT_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Authenticates the user by an external identity provider token.
    /// </summary>
    public async Task<ResponseDto<LoginResponse>> LoginByThirdPartyAsync(ThirdPartyLoginCommand command, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(command.Provider);
        if (provider is null)
        {
            return new ResponseDto<LoginResponse>(AUTH_EXTERNAL_PROVIDER_INVALID, INVALID_EXTERNAL_PROVIDER_MESSAGE);
        }

        var profile = await ValidateExternalTokenAsync(provider, command.ExternalToken, cancellationToken);
        if (profile is null)
        {
            return new ResponseDto<LoginResponse>(AUTH_EXTERNAL_PROVIDER_INVALID, INVALID_EXTERNAL_TOKEN_MESSAGE);
        }

        var existingMapping = await _externalLoginRepository.GetByProviderAsync(provider.Name, profile.ProviderUserId, cancellationToken);

        if (existingMapping is not null)
        {
            if (!CanLogin(existingMapping.User))
            {
                return new ResponseDto<LoginResponse>(AUTH_ACCOUNT_INACTIVE, ACCOUNT_INACTIVE_MESSAGE);
            }

            existingMapping.User.LastLoginAt = DateTime.UtcNow;
            return await CreateLoginResponseAsync(existingMapping.User, cancellationToken);
        }

        var normalizedEmail = NormalizeEmail(profile.Email);
        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
            if (existingUser is not null)
            {
                return new ResponseDto<LoginResponse>(
                    AUTH_EXTERNAL_PROVIDER_LINK_CONFLICT,
                    ACCOUNT_ALREADY_EXISTS_LINK_MESSAGE);
            }
        }

        var createdUser = await CreateExternalUserAsync(provider, profile, cancellationToken);
        createdUser.LastLoginAt = DateTime.UtcNow;
        return await CreateLoginResponseAsync(createdUser, cancellationToken);
    }

    /// <summary>
    /// Sends an OTP email for the forgot-password flow without revealing account existence.
    /// </summary>
    public async Task<ResponseDto<string>> ForgotPasswordAsync(ForgotPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(command.Email);
        var throttleError = await CheckOtpThrottleAsync(FORGOT_PASSWORD_PURPOSE, normalizedEmail, FORGOT_PASSWORD_OTP_LIMIT, cancellationToken);
        if (throttleError is not null)
        {
            return new ResponseDto<string>(FORGOT_PASSWORD_SUCCESS_MESSAGE);
        }

        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null)
        {
            await SetCooldownAsync(FORGOT_PASSWORD_PURPOSE, normalizedEmail, cancellationToken);
            return new ResponseDto<string>(FORGOT_PASSWORD_SUCCESS_MESSAGE);
        }

        var otpCode = GenerateOtp();
        await SetOtpAsync(FORGOT_PASSWORD_PURPOSE, normalizedEmail, otpCode, cancellationToken);
        var sent = await SendOtpEmailAsync(normalizedEmail, otpCode, EMAIL_SUBJECT_RESET_PASSWORD, FORGOT_PASSWORD_PURPOSE, cancellationToken);
        if (sent)
        {
            await SetCooldownAsync(FORGOT_PASSWORD_PURPOSE, normalizedEmail, cancellationToken);
            _logger.LogInformation(LOG_FORGOT_PASSWORD_OTP_SENT);
        }
        else
        {
            await RemoveCacheAsync(GetOtpKey(FORGOT_PASSWORD_PURPOSE, normalizedEmail), cancellationToken);
            _logger.LogWarning(LOG_FORGOT_PASSWORD_OTP_SEND_FAILED);
        }

        return new ResponseDto<string>(FORGOT_PASSWORD_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Verifies the forgot-password OTP and creates a short-lived reset session in Redis.
    /// </summary>
    public async Task<ResponseDto<string>> VerifyForgotPasswordOtpAsync(VerifyForgotPasswordOtpCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(command.Email);
        var otpVerification = await VerifyOtpAsync(FORGOT_PASSWORD_PURPOSE, normalizedEmail, command.Otp, cancellationToken);
        if (!otpVerification.Success)
        {
            return otpVerification;
        }

        await SetCacheAsync(
            GetResetSessionKey(normalizedEmail),
            new ResetSessionCacheEntry { CreatedAt = DateTime.UtcNow },
            TimeSpan.FromMinutes(RESET_SESSION_TTL_MINUTES),
            cancellationToken);
        _logger.LogInformation(LOG_FORGOT_PASSWORD_OTP_VERIFIED);
        return new ResponseDto<string>(OTP_VERIFIED_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Changes the password after a successful forgot-password OTP verification.
    /// </summary>
    public async Task<ResponseDto<string>> ChangeForgotPasswordAsync(ChangeForgotPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(command.Email);
        var resetSession = await GetCacheAsync<ResetSessionCacheEntry>(GetResetSessionKey(normalizedEmail), cancellationToken);
        if (resetSession is null)
        {
            return new ResponseDto<string>(AUTH_RESET_SESSION_INVALID, RESET_SESSION_INVALID_MESSAGE);
        }

        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null)
        {
            return new ResponseDto<string>(AUTH_RESET_SESSION_INVALID, RESET_SESSION_INVALID_MESSAGE);
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, command.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await RevokeAllRefreshTokensAsync(user.Id, cancellationToken, false);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await RemoveCacheAsync(GetResetSessionKey(normalizedEmail), cancellationToken);

        _logger.LogInformation(LOG_FORGOT_PASSWORD_CHANGED, user.PublicId);
        return new ResponseDto<string>(PASSWORD_CHANGED_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Changes the password for the current authenticated user and revokes existing refresh tokens.
    /// </summary>
    public async Task<ResponseDto<string>> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        _authService.EnsureAuthenticated();
        if (!Guid.TryParse(_authService.UserId(), out var userPublicId))
        {
            return new ResponseDto<string>(AUTH_UNAUTHORIZED, UNAUTHORIZED_REQUEST_MESSAGE);
        }

        var user = await _userRepository.GetByPublicIdAsync(userPublicId, cancellationToken);
        if (user is null)
        {
            return new ResponseDto<string>(AUTH_UNAUTHORIZED, UNAUTHORIZED_REQUEST_MESSAGE);
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.CurrentPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            return new ResponseDto<string>(AUTH_INVALID_CREDENTIALS, CURRENT_PASSWORD_INVALID_MESSAGE);
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, command.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await RevokeAllRefreshTokensAsync(user.Id, cancellationToken, false);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(LOG_PASSWORD_CHANGED, user.PublicId);
        return new ResponseDto<string>(PASSWORD_CHANGED_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Links an external provider to the current authenticated user.
    /// </summary>
    public async Task<ResponseDto<string>> LinkExternalProviderAsync(LinkExternalProviderCommand command, CancellationToken cancellationToken = default)
    {
        _authService.EnsureAuthenticated();
        if (!Guid.TryParse(_authService.UserId(), out var userPublicId))
        {
            return new ResponseDto<string>(AUTH_UNAUTHORIZED, UNAUTHORIZED_REQUEST_MESSAGE);
        }

        var user = await _userRepository.GetByPublicIdAsync(userPublicId, cancellationToken);
        if (user is null)
        {
            return new ResponseDto<string>(AUTH_UNAUTHORIZED, UNAUTHORIZED_REQUEST_MESSAGE);
        }

        var provider = GetProvider(command.Provider);
        if (provider is null)
        {
            return new ResponseDto<string>(AUTH_EXTERNAL_PROVIDER_INVALID, INVALID_EXTERNAL_PROVIDER_MESSAGE);
        }

        var profile = await ValidateExternalTokenAsync(provider, command.ExternalToken, cancellationToken);
        if (profile is null)
        {
            return new ResponseDto<string>(AUTH_EXTERNAL_PROVIDER_INVALID, INVALID_EXTERNAL_TOKEN_MESSAGE);
        }

        var existingMapping = await _externalLoginRepository.GetByProviderAsync(provider.Name, profile.ProviderUserId, cancellationToken);

        if (existingMapping is not null && existingMapping.UserId != user.Id)
        {
            return new ResponseDto<string>(AUTH_EXTERNAL_PROVIDER_LINK_CONFLICT, EXTERNAL_PROVIDER_LINK_CONFLICT_MESSAGE);
        }

        if (existingMapping is null)
        {
            await _externalLoginRepository.AddAsync(new ExternalLogin
            {
                UserId = user.Id,
                LoginProvider = provider.Name,
                ProviderKey = profile.ProviderUserId
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(LOG_EXTERNAL_PROVIDER_LINKED, provider.Name, user.PublicId);
        return new ResponseDto<string>(EXTERNAL_PROVIDER_LINKED_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Loads the current authenticated user profile, role list, permissions, and external providers.
    /// </summary>
    public async Task<ResponseDto<UserInfoResponse>> GetUserInfoAsync(CancellationToken cancellationToken = default)
    {
        _authService.EnsureAuthenticated();
        if (!Guid.TryParse(_authService.UserId(), out var userPublicId))
        {
            return new ResponseDto<UserInfoResponse>(AUTH_UNAUTHORIZED, UNAUTHORIZED_REQUEST_MESSAGE);
        }

        var user = await _userRepository.GetUserInfoByPublicIdAsync(userPublicId, cancellationToken);

        if (user is null)
        {
            return new ResponseDto<UserInfoResponse>(AUTH_UNAUTHORIZED, UNAUTHORIZED_REQUEST_MESSAGE);
        }

        var response = new UserInfoResponse
        {
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DisplayName = user.Party?.DisplayName
        };

        response.Roles = user.UserRoles
            .Where(x => x.Role is not null && !x.Role.IsDeleted)
            .Select(x => string.IsNullOrWhiteSpace(x.Role.Code) ? x.Role.Name : x.Role.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        response.Permissions = user.UserRoles
            .Where(x => x.Role is not null && !x.Role.IsDeleted)
            .SelectMany(x => x.Role.RolePermissions)
            .Where(x => x.Permission is not null && !x.Permission.IsDeleted)
            .Select(x => string.IsNullOrWhiteSpace(x.Permission.Code) ? x.Permission.Name : x.Permission.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        response.ExternalProviders = user.ExternalLogins
            .GroupBy(x => x.LoginProvider, StringComparer.OrdinalIgnoreCase)
            .Select(x => new ExternalProviderDto { Provider = x.Key })
            .ToList();

        return new ResponseDto<UserInfoResponse>(response);
    }

    private async Task<User> CreateExternalUserAsync(ExternalProviderOptions provider, ExternalIdentityProfile profile, CancellationToken cancellationToken)
    {
        var activeStatus = await FindMasterDataValueAsync(STATUS_TYPE, ACTIVE_STATUS, cancellationToken)
            ?? throw new InvalidOperationException(ACTIVE_STATUS_NOT_FOUND_MESSAGE);
        var partyType = await FindMasterDataValueAsync(PARTY_TYPE_TYPE, provider.DefaultPartyType, cancellationToken)
            ?? throw new InvalidOperationException(DEFAULT_PARTY_TYPE_NOT_FOUND_MESSAGE);
        var role = await FindRoleForPartyTypeAsync(provider.DefaultPartyType, cancellationToken)
            ?? throw new InvalidOperationException(DEFAULT_ROLE_NOT_FOUND_MESSAGE);

        var fullName = string.IsNullOrWhiteSpace(profile.FullName) ? profile.Email ?? profile.ProviderUserId : profile.FullName.Trim();
        var normalizedEmail = NormalizeEmail(profile.Email);

        var party = new Party
        {
            PartyTypeId = partyType.Id,
            DisplayName = fullName,
            LegalName = fullName,
            PrimaryEmail = normalizedEmail,
            StatusId = activeStatus.Id
        };

        var user = new User
        {
            UserName = await GenerateUniqueUserNameAsync(normalizedEmail, fullName, cancellationToken),
            Email = normalizedEmail,
            FullName = fullName,
            EmailConfirmed = string.IsNullOrWhiteSpace(normalizedEmail) || !provider.RequireVerifiedEmail || profile.EmailVerified,
            StatusId = activeStatus.Id,
            LockoutEnabled = true
        };

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _partyRepository.AddAsync(party, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            user.PartyId = party.Id;

            await _userRepository.AddAsync(user, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            await _externalLoginRepository.AddAsync(new ExternalLogin
            {
                UserId = user.Id,
                LoginProvider = provider.Name,
                ProviderKey = profile.ProviderUserId
            }, ct);

            await _userRoleRepository.AddAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            }, ct);
        }, cancellationToken);

        return await _userRepository.GetUserForAuthenticationByIdAsync(user.Id, cancellationToken)
            ?? throw new InvalidOperationException(EXTERNAL_USER_NOT_LOADED_MESSAGE);
    }

    private async Task<ResponseDto<string>> VerifyOtpAsync(string purpose, string normalizedEmail, string otp, CancellationToken cancellationToken)
    {
        var otpKey = GetOtpKey(purpose, normalizedEmail);
        var otpEntry = await GetCacheAsync<OtpCacheEntry>(otpKey, cancellationToken);
        if (otpEntry is null || string.IsNullOrWhiteSpace(otpEntry.Code))
        {
            return new ResponseDto<string>(AUTH_OTP_INVALID, OTP_INVALID_OR_EXPIRED_MESSAGE);
        }

        var remainingTtl = otpEntry.ExpiresAtUtc - DateTime.UtcNow;
        if (remainingTtl <= TimeSpan.Zero)
        {
            await RemoveCacheAsync(otpKey, cancellationToken);
            return new ResponseDto<string>(AUTH_OTP_INVALID, OTP_INVALID_OR_EXPIRED_MESSAGE);
        }

        if (!string.Equals(otpEntry.Code, otp?.Trim(), StringComparison.Ordinal))
        {
            otpEntry.Attempts++;
            if (otpEntry.Attempts >= OTP_MAX_VERIFY_ATTEMPTS)
            {
                await RemoveCacheAsync(otpKey, cancellationToken);
            }
            else
            {
                await SetCacheAsync(otpKey, otpEntry, remainingTtl, cancellationToken);
            }

            return new ResponseDto<string>(AUTH_OTP_INVALID, OTP_INVALID_OR_EXPIRED_MESSAGE);
        }

        await RemoveCacheAsync(otpKey, cancellationToken);
        return new ResponseDto<string>(OTP_VERIFIED_SUCCESS_MESSAGE);
    }

    private async Task<ResponseDto<string>> CheckRegistrationUniquenessAsync(string userName, string email, string phoneNumber, CancellationToken cancellationToken)
    {
        if (await _userRepository.UserNameExistsAsync(userName, cancellationToken))
        {
            return new ResponseDto<string>(AUTH_USER_ALREADY_EXISTS, USERNAME_ALREADY_EXISTS_MESSAGE);
        }

        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
        {
            return new ResponseDto<string>(AUTH_USER_ALREADY_EXISTS, EMAIL_ALREADY_EXISTS_MESSAGE);
        }

        if (await _userRepository.PhoneNumberExistsAsync(phoneNumber.Trim(), cancellationToken))
        {
            return new ResponseDto<string>(AUTH_USER_ALREADY_EXISTS, PHONE_NUMBER_ALREADY_EXISTS_MESSAGE);
        }

        return null;
    }

    private async Task<ResponseDto<string>> CheckOtpThrottleAsync(string purpose, string normalizedEmail, int limit, CancellationToken cancellationToken)
    {
        var cooldownKey = GetCooldownKey(purpose, normalizedEmail);
        if (!string.IsNullOrWhiteSpace(await GetCacheAsync<string>(cooldownKey, cancellationToken)))
        {
            return new ResponseDto<string>(AUTH_OTP_COOLDOWN, OTP_COOLDOWN_MESSAGE);
        }

        var limitKey = GetLimitKey(purpose, normalizedEmail);
        var currentLimit = await GetCacheAsync<int?>(limitKey, cancellationToken) ?? 0;
        currentLimit++;
        if (currentLimit > limit)
        {
            return new ResponseDto<string>(AUTH_OTP_RATE_LIMIT, OTP_RATE_LIMIT_MESSAGE);
        }

        await SetCacheAsync(limitKey, currentLimit, TimeSpan.FromMinutes(OTP_LIMIT_TTL_MINUTES), cancellationToken);
        return null;
    }

    private async Task SetCooldownAsync(string purpose, string normalizedEmail, CancellationToken cancellationToken)
    {
        await _cachingService.SetAbsoluteAsync(
            GetCooldownKey(purpose, normalizedEmail),
            OTP_COOLDOWN_VALUE,
            TimeSpan.FromSeconds(OTP_COOLDOWN_SECONDS),
            cancellationToken);
    }

    private async Task<bool> SendOtpEmailAsync(string email, string otpCode, string subject, string purpose, CancellationToken cancellationToken)
    {
        var emailRequest = new EmailRequest
        {
            RequestData = new RequestData
            {
                Subject = subject,
                Body = string.Format(OTP_EMAIL_HTML_TEMPLATE, otpCode),
                To =
                [
                    new EmailAddressRequest { Email = email }
                ]
            }
        };

        var sent = await _emailService.SendEmailAsync(emailRequest);
        if (sent)
        {
            return true;
        }

        _logger.LogWarning(LOG_SENDGRID_OTP_SEND_FAILED, purpose);
        return false;
    }

    private async Task<ExternalIdentityProfile> ValidateExternalTokenAsync(ExternalProviderOptions provider, string externalToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(externalToken))
        {
            return null;
        }

        try
        {
            var metadataAddress = string.IsNullOrWhiteSpace(provider.MetadataAddress)
                ? $"{provider.Authority.TrimEnd('/')}/.well-known/openid-configuration"
                : provider.MetadataAddress;
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                metadataAddress,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = metadataAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase) });
            var configuration = await configurationManager.GetConfigurationAsync(cancellationToken);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = configuration.SigningKeys,
                ValidateIssuer = true,
                ValidIssuers = provider.ValidIssuers.Count > 0 ? provider.ValidIssuers : [configuration.Issuer],
                ValidateAudience = provider.Audiences.Count > 0,
                ValidAudiences = provider.Audiences,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            var principal = _jwtSecurityTokenHandler.ValidateToken(externalToken, validationParameters, out _);
            var email = principal.FindFirstValue(ClaimTypes.Email)
                        ?? principal.FindFirstValue("email");
            var fullName = principal.FindFirstValue(ClaimTypes.Name)
                           ?? principal.FindFirstValue("name")
                           ?? principal.FindFirstValue("given_name");
            var providerUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? principal.FindFirstValue("sub");
            var emailVerified = bool.TryParse(principal.FindFirstValue("email_verified"), out var verified) && verified;

            if (string.IsNullOrWhiteSpace(providerUserId))
            {
                return null;
            }

            if (provider.RequireVerifiedEmail && !string.IsNullOrWhiteSpace(email) && !emailVerified)
            {
                return null;
            }

            return new ExternalIdentityProfile
            {
                ProviderUserId = providerUserId,
                Email = email,
                FullName = fullName,
                EmailVerified = emailVerified || !provider.RequireVerifiedEmail
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, LOG_EXTERNAL_TOKEN_VALIDATION_FAILED, provider.Name);
            return null;
        }
    }

    private async Task<LoginResponse> BuildLoginResponseAsync(User user, string refreshToken, CancellationToken cancellationToken)
    {
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(_authOptions.AccessTokenMinutes);
        var jwtId = Guid.NewGuid().ToString("N");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.PublicId.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, jwtId)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        var permissionCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var userRole in user.UserRoles.Where(x => x.Role is not null && !x.Role.IsDeleted))
        {
            var roleValue = string.IsNullOrWhiteSpace(userRole.Role.Code) ? userRole.Role.Name : userRole.Role.Code;
            if (!string.IsNullOrWhiteSpace(roleValue))
            {
                claims.Add(new Claim(ClaimTypes.Role, roleValue));
            }

            foreach (var rolePermission in userRole.Role.RolePermissions.Where(x => x.Permission is not null && !x.Permission.IsDeleted))
            {
                var permissionValue = string.IsNullOrWhiteSpace(rolePermission.Permission.Code)
                    ? rolePermission.Permission.Name
                    : rolePermission.Permission.Code;
                if (!string.IsNullOrWhiteSpace(permissionValue) && permissionCodes.Add(permissionValue))
                {
                    claims.Add(new Claim("permission", permissionValue));
                }
            }
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _authOptions.Issuer,
            audience: _authOptions.Audiences.FirstOrDefault(),
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: credentials);

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            JwtId = jwtId,
            ExpiresAt = issuedAt.AddDays(_authOptions.RefreshTokenDays)
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = _jwtSecurityTokenHandler.WriteToken(jwt),
            RefreshToken = refreshToken,
            ExpiresIn = (int)TimeSpan.FromMinutes(_authOptions.AccessTokenMinutes).TotalSeconds,
            TokenType = TOKEN_TYPE_BEARER
        };
    }

    private async Task<ResponseDto<LoginResponse>> CreateLoginResponseAsync(User user, CancellationToken cancellationToken)
    {
        var rawRefreshToken = GenerateRefreshToken();
        var response = await BuildLoginResponseAsync(user, rawRefreshToken, cancellationToken);
        return new ResponseDto<LoginResponse>(response);
    }

    private async Task RevokeAllRefreshTokensAsync(long userId, CancellationToken cancellationToken, bool saveChanges = true)
    {
        var activeTokens = await _refreshTokenRepository.GetActiveByUserIdAsync(userId, cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        if (saveChanges)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<MasterDataValue> FindMasterDataValueAsync(string type, string value, CancellationToken cancellationToken)
    {
        return await _masterDataValueRepository.GetByTypeAndValueAsync(type, value, cancellationToken);
    }

    private async Task<Role> FindRoleForPartyTypeAsync(string partyType, CancellationToken cancellationToken)
    {
        return await _roleRepository.GetByPartyTypeAsync(partyType, cancellationToken);
    }

    private async Task<string> GenerateUniqueUserNameAsync(string email, string fullName, CancellationToken cancellationToken)
    {
        var baseName = !string.IsNullOrWhiteSpace(email)
            ? email.Split('@')[0]
            : fullName.Replace(" ", string.Empty, StringComparison.Ordinal);
        baseName = NormalizeUserName(baseName);

        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = DEFAULT_EXTERNAL_USER_NAME;
        }

        var candidate = baseName;
        var suffix = 1;
        while (await _userRepository.UserNameExistsAsync(candidate, cancellationToken))
        {
            suffix++;
            candidate = $"{baseName}{suffix}";
        }

        return candidate;
    }

    private ExternalProviderOptions GetProvider(string provider)
    {
        var normalizedProvider = provider?.Trim();
        return _externalAuthenticationOptions.Providers
            .FirstOrDefault(x => string.Equals(x.Name, normalizedProvider, StringComparison.OrdinalIgnoreCase));
    }

    private static bool CanLogin(User user)
    {
        return user is not null
               && !user.IsDeleted
               && user.Status is not null
               && (string.Equals(user.Status.Code, ACTIVE_STATUS, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(user.Status.Name, ACTIVE_STATUS, StringComparison.OrdinalIgnoreCase))
               && user.EmailConfirmed;
    }

    private static string NormalizeEmail(string email)
    {
        return email?.Trim().ToLowerInvariant();
    }

    private static string NormalizeUserName(string userName)
    {
        return userName?.Trim();
    }

    private static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    private static string GenerateOtp()
    {
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString($"D{OTP_LENGTH}");
    }

    private static string GetOtpKey(string purpose, string email) => string.Format(OTP_KEY_PATTERN, purpose, email);

    private static string GetPendingRegisterKey(string email) => string.Format(PENDING_REGISTER_KEY_PATTERN, email);

    private static string GetCooldownKey(string purpose, string email) => string.Format(OTP_COOLDOWN_KEY_PATTERN, purpose, email);

    private static string GetLimitKey(string purpose, string email) => string.Format(OTP_LIMIT_KEY_PATTERN, purpose, email);

    private static string GetResetSessionKey(string email) => string.Format(RESET_SESSION_KEY_PATTERN, email);

    private Task SetOtpAsync(string purpose, string normalizedEmail, string otpCode, CancellationToken cancellationToken)
    {
        var otpEntry = new OtpCacheEntry
        {
            Code = otpCode,
            Attempts = 0,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(OTP_TTL_MINUTES)
        };

        return SetCacheAsync(
            GetOtpKey(purpose, normalizedEmail),
            otpEntry,
            TimeSpan.FromMinutes(OTP_TTL_MINUTES),
            cancellationToken);
    }

    private async Task SetCacheAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        await _cachingService.SetAbsoluteAsync(key, value, ttl, cancellationToken);
    }

    private async Task<T> GetCacheAsync<T>(string key, CancellationToken cancellationToken)
    {
        return await _cachingService.GetAsync<T>(key, cancellationToken);
    }

    private async Task RemoveCacheAsync(string key, CancellationToken cancellationToken)
    {
        await _cachingService.RemoveAsync(key, cancellationToken);
    }
}
