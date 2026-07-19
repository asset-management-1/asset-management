namespace Authentication.Infrastructure.Services;

/// <summary>
/// Provides infrastructure operations for local authentication flows.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly AuthenticationRepositoryDependencies _repositories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ICachingService _cachingService;
    private readonly AuthOptions _authOptions;
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IClientDeviceContextAccessor _clientDeviceContextAccessor;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();

    /// <summary>
    /// Creates the local authentication service with persistence, email, password hashing, and token options.
    /// </summary>
    /// <param name="repositories">The grouped authentication repositories used by persistence operations.</param>
    /// <param name="supportDependencies">The grouped support dependencies used by local authentication flows.</param>
    /// <param name="clientDeviceContextAccessor">The accessor used to capture device metadata for token issuance.</param>
    /// <param name="logger">The local authentication service logger.</param>
    public AuthenticationService(
        AuthenticationRepositoryDependencies repositories,
        AuthenticationServiceSupportDependencies supportDependencies,
        IClientDeviceContextAccessor clientDeviceContextAccessor,
        ILogger<AuthenticationService> logger)
    {
        // Split repository and support dependency groups keep the service constructor within analyzer limits.
        _repositories = repositories;
        _unitOfWork = supportDependencies.UnitOfWork;
        _emailService = supportDependencies.EmailService;
        _passwordHasher = supportDependencies.PasswordHasher;
        _cachingService = supportDependencies.CachingService;
        _authOptions = supportDependencies.AuthOptions;
        _emailOptions = supportDependencies.EmailOptions;
        _logger = logger;
        _clientDeviceContextAccessor = clientDeviceContextAccessor;
    }

    /// <summary>
    /// Authenticates a local username/password login and issues a token pair.
    /// </summary>
    /// <param name="request">The local login request payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The issued access and refresh token payload.</returns>
    public async Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Load the minimal password identity and validate local credentials before issuing tokens.
        var user = await _repositories.UserRepository.GetUserForAuthenticationByUserNameAsync(
            request.UserName,
            cancellationToken);
        if (user is null
            || user.IsDeleted
            || user.Status is null
            || !string.Equals(user.Status.Code, ACTIVE_STATUS, StringComparison.OrdinalIgnoreCase)
            || !user.EmailConfirmed
            || string.IsNullOrWhiteSpace(user.PasswordHash)
            || _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password) == PasswordVerificationResult.Failed)
        {
            throw new ApiException(
                ApplicationErrorConstants.AccountErrors.INVALID_USERNAME_OR_PASSWORD_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_INVALID_CREDENTIALS,
                StatusCodes.Status401Unauthorized);
        }

        // Stage login state and persist it together with the client-session refresh token.
        await _repositories.UserRepository.StageLastLoginAtAsync(user.Id, DateTime.UtcNow);

        var deviceContext = _clientDeviceContextAccessor.GetCurrent();
        var sessionRefreshToken = await _repositories.RefreshTokenRepository.GetClientSessionByUserAndDeviceIdAsync(
            user.Id,
            deviceContext.DeviceId,
            cancellationToken);
        var isNewClientSession = sessionRefreshToken is null;
        sessionRefreshToken ??= new RefreshToken();

        // Resolve the Party context independently for this client session before issuing its token pair.
        sessionRefreshToken.CurrentPartyId = await _repositories.UserPartyRepository.ResolveSessionPartyIdAsync(
                                                 user.Id,
                                                 sessionRefreshToken.CurrentPartyId,
                                                 cancellationToken)
                                             ?? throw new ApiException(
                                                 ApplicationErrorConstants.ContextErrors.PARTY_CONTEXT_NOT_AVAILABLE_MESSAGE,
                                                 ApplicationErrorConstants.AccountErrorCodes.AUTH_PARTY_CONTEXT_NOT_AVAILABLE);

        // Issue the access token and rotate or create the persistent refresh-token session in one flow.
        var response = await IssueAsync(
            new AuthSessionIssueRequestModel
            {
                User = user,
                RawRefreshToken = AuthSessionHelper.GenerateRefreshToken(),
                SessionRefreshToken = sessionRefreshToken,
                DeviceContext = deviceContext,
                RenewSessionPublicId = true,
                AuthOptions = _authOptions,
                JwtSecurityTokenHandler = _jwtSecurityTokenHandler
            },
            cancellationToken);
        _logger.LogInformation(
            isNewClientSession
                ? InfrastructureLogConstants.SessionLogs.CLIENT_SESSION_CREATED
                : InfrastructureLogConstants.SessionLogs.CLIENT_SESSION_REPLACED_BY_LOGIN,
            sessionRefreshToken.SessionPublicId,
            user.PublicId);
        return response;
    }

    /// <summary>
    /// Builds the pending registration payload after validating uniqueness and hashing the password.
    /// </summary>
    /// <param name="request">The normalized registration request payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The pending registration payload to cache for email verification.</returns>
    public async Task<PendingRegisterCacheRequestDto> BuildPendingRegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Validate uniqueness and target party type before the pending registration is cached.
        await EnsureRegistrationCanStartAsync(
            request.Adapt<RegistrationUniquenessRequestDto>(),
            request.PartyType,
            cancellationToken);

        // Hash the password before caching so the raw password never leaves the current request scope.
        var passwordUser = request.Adapt<User>();
        var pendingRegister = request.Adapt<PendingRegisterCacheRequestDto>();
        pendingRegister.PasswordHash = _passwordHasher.HashPassword(passwordUser, request.Password);
        pendingRegister.CreatedAt = DateTime.UtcNow;

        return pendingRegister;
    }

    /// <summary>
    /// Validates that a registration request can still create a unique account for the target party type.
    /// </summary>
    /// <param name="request">The normalised uniqueness values from the registration payload.</param>
    /// <param name="partyType">The normalised party type master-data value.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that completes when the registration can proceed.</returns>
    private async Task EnsureRegistrationCanStartAsync(
        RegistrationUniquenessRequestDto request,
        string partyType,
        CancellationToken cancellationToken)
    {
        // Check account uniqueness first so duplicate registrations stop before master-data lookup.
        await EnsureRegistrationUniquenessAsync(request, cancellationToken);

        // Resolve the target party type from master data to prevent unsupported context creation.
        var partyTypeValue = await _repositories.MasterDataValueRepository.GetByTypeAndValueAsync(
            PARTY_TYPE_TYPE,
            partyType,
            cancellationToken);
        if (partyTypeValue is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.ContextErrors.INVALID_PARTY_TYPE_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_FORBIDDEN_OPERATION);
        }
    }

    /// <summary>
    /// Validates that username, email, and phone number can create a unique account.
    /// </summary>
    /// <param name="request">The normalised uniqueness values from the registration payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that completes when uniqueness checks pass.</returns>
    private async Task EnsureRegistrationUniquenessAsync(
        RegistrationUniquenessRequestDto request,
        CancellationToken cancellationToken)
    {
        // Each uniqueness check maps to a database constraint the registration flow must respect.
        if (await _repositories.UserRepository.UserNameExistsAsync(request.UserName, cancellationToken))
        {
            throw new ApiException(
                ApplicationErrorConstants.AccountErrors.USERNAME_ALREADY_EXISTS_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_USER_ALREADY_EXISTS);
        }

        if (await _repositories.UserRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new ApiException(
                ApplicationErrorConstants.AccountErrors.EMAIL_ALREADY_EXISTS_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_USER_ALREADY_EXISTS);
        }

        if (await _repositories.UserRepository.PhoneNumberExistsAsync(request.PhoneNumber, cancellationToken))
        {
            throw new ApiException(
                ApplicationErrorConstants.AccountErrors.PHONE_NUMBER_ALREADY_EXISTS_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_USER_ALREADY_EXISTS);
        }
    }

    /// <summary>
    /// Determines whether a non-deleted user exists for the supplied email.
    /// </summary>
    /// <param name="normalizedEmail">The normalised email address.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns><c>true</c> when a matching user exists; otherwise <c>false</c>.</returns>
    public async Task<bool> UserExistsByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        // Use the repository's minimal email projection for forgot-password enumeration-safe checks.
        return await _repositories.UserRepository.GetByEmailAsync(normalizedEmail, cancellationToken) is not null;
    }

    /// <summary>
    /// Completes account creation from the pending register payload.
    /// </summary>
    /// <param name="pendingRegister">The verified pending registration payload from cache.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The registration completion result.</returns>
    public async Task<OperationStatusResponseDto> CompleteRegistrationAsync(
        PendingRegisterCacheRequestDto pendingRegister,
        CancellationToken cancellationToken = default)
    {
        // Re-check uniqueness at verify time to protect against races while the OTP was pending.
        await EnsureRegistrationUniquenessAsync(
            pendingRegister.Adapt<RegistrationUniquenessRequestDto>(),
            cancellationToken);

        // Resolve all creation statuses and context type in one master-data batch.
        var masterDataValues = await _repositories.MasterDataValueRepository.GetByTypeAndValuesAsync(
            [
                new MasterDataValueKeyModel(PARTY_TYPE_TYPE, pendingRegister.PartyType),
                new MasterDataValueKeyModel(PARTY_STATUS_TYPE, ACTIVE_STATUS),
                new MasterDataValueKeyModel(USER_STATUS_TYPE, ACTIVE_STATUS)
            ],
            cancellationToken);
        var partyType = MasterDataValueHelper.GetRequired(
            masterDataValues,
            new MasterDataValueRequirementModel(
                PARTY_TYPE_TYPE,
                pendingRegister.PartyType,
                ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_USER_STATUS_NOT_FOUND));
        var partyStatus = MasterDataValueHelper.GetRequired(
            masterDataValues,
            new MasterDataValueRequirementModel(
                PARTY_STATUS_TYPE,
                ACTIVE_STATUS,
                ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_USER_STATUS_NOT_FOUND));
        var userStatus = MasterDataValueHelper.GetRequired(
            masterDataValues,
            new MasterDataValueRequirementModel(
                USER_STATUS_TYPE,
                ACTIVE_STATUS,
                ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_USER_STATUS_NOT_FOUND));

        var provision = pendingRegister.Adapt<RegisterAccountProvisionRequestDto>();
        provision.PartyTypeId = partyType.Id;
        provision.PartyStatusId = partyStatus.Id;
        provision.UserStatusId = userStatus.Id;

        // The created user is captured for a completion log after the transaction succeeds.
        User createdUser = null;

        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(
                async ct =>
                {
                    // Step 1: Recheck identity uniqueness inside the transaction before staging the graph.
                    await EnsureRegistrationUniquenessAsync(
                        pendingRegister.Adapt<RegistrationUniquenessRequestDto>(),
                        ct);

                    // Step 2: Create Party, User, and UserParty as one atomic identity graph.
                    var party = provision.Adapt<Party>();
                    var user = provision.Adapt<User>();

                    await _repositories.PartyRepository.AddAsync(party, ct);
                    await _repositories.UserRepository.AddAsync(user, ct);
                    await _repositories.UserPartyRepository.AddAsync(new UserParty { User = user, Party = party }, ct);
                    createdUser = user;
                },
                cancellationToken);
        }
        catch (DbUpdateException exception)
            when (PostgreSqlExceptionHelper.GetUniqueConstraintName(exception)
                is USER_NAME_UNIQUE_CONSTRAINT or USER_EMAIL_UNIQUE_CONSTRAINT or USER_PHONE_UNIQUE_CONSTRAINT)
        {
            // Step 3: Convert the database race winner into the same stable duplicate-account contract.
            throw CreateRegistrationConflictException(exception);
        }

        _logger.LogInformation(
            InfrastructureLogConstants.SessionLogs.REGISTER_COMPLETED,
            createdUser?.PublicId);

        return OperationStatusResponseHelper.Success(
            ApplicationMessageConstants.AccountMessages.REGISTRATION_COMPLETED_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Maps a concurrent registration unique violation to the matching account error.
    /// </summary>
    /// <param name="exception">The EF Core exception raised while committing registration.</param>
    /// <returns>The stable business exception for the violated identity field.</returns>
    private static ApiException CreateRegistrationConflictException(DbUpdateException exception)
    {
        return PostgreSqlExceptionHelper.GetUniqueConstraintName(exception) switch
        {
            USER_NAME_UNIQUE_CONSTRAINT => new ApiException(
                ApplicationErrorConstants.AccountErrors.USERNAME_ALREADY_EXISTS_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_USER_ALREADY_EXISTS),
            USER_EMAIL_UNIQUE_CONSTRAINT => new ApiException(
                ApplicationErrorConstants.AccountErrors.EMAIL_ALREADY_EXISTS_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_USER_ALREADY_EXISTS),
            USER_PHONE_UNIQUE_CONSTRAINT => new ApiException(
                ApplicationErrorConstants.AccountErrors.PHONE_NUMBER_ALREADY_EXISTS_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_USER_ALREADY_EXISTS),
            _ => new ApiException(
                ApplicationErrorConstants.AccountErrors.USERNAME_ALREADY_EXISTS_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_USER_ALREADY_EXISTS)
        };
    }

    /// <summary>
    /// Sends one OTP email.
    /// </summary>
    /// <param name="request">The OTP email payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The OTP email send result.</returns>
    public async Task<OtpEmailResponseDto> SendOtpEmailAsync(
        OtpEmailRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Send the OTP through the shared email service without logging the OTP value.
        var sent = await _emailService.SendEmailAsync(
            new EmailRequest
            {
                RequestData = new RequestData
                {
                    Subject = request.Subject,
                    Body = string.Format(
                        InfrastructureMessageConstants.EmailMessages.OTP_EMAIL_HTML_TEMPLATE,
                        request.OtpCode),
                    To =
                    [
                        new EmailAddressRequest { Email = request.Email }
                    ]
                }
            },
            cancellationToken);

        if (!sent)
        {
            // Log only the OTP purpose so no recipient or code leaks into infrastructure logs.
            _logger.LogWarning(
                InfrastructureLogConstants.EmailLogs.OTP_SEND_FAILED,
                request.Purpose);
        }

        return sent.Adapt<OtpEmailResponseDto>();
    }

    /// <summary>
    /// Sends a security notification to the old email when a change-email request starts.
    /// </summary>
    /// <param name="request">The security notification payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The email send result.</returns>
    public async Task<OtpEmailResponseDto> SendChangeEmailSecurityNotificationAsync(
        ChangeEmailSecurityNotificationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Accounts without an old email cannot receive a security notification.
        if (string.IsNullOrWhiteSpace(request.OldEmail))
        {
            _logger.LogInformation(InfrastructureLogConstants.EmailLogs.CHANGE_EMAIL_SECURITY_NOTIFICATION_SKIPPED);
            return false.Adapt<OtpEmailResponseDto>();
        }

        // Prefer the configured support mailbox so suspicious change-email attempts can be reported.
        var supportEmail = string.IsNullOrWhiteSpace(_emailOptions.SystemSupportEmail)
            ? _emailOptions.FromEmail
            : _emailOptions.SystemSupportEmail;
        var sent = await _emailService.SendEmailAsync(
            new EmailRequest
            {
                RequestData = new RequestData
                {
                    Subject = ApplicationMessageConstants.EmailMessages.EMAIL_SUBJECT_CHANGE_EMAIL_SECURITY,
                    Body = string.Format(
                        InfrastructureMessageConstants.EmailMessages.CHANGE_EMAIL_SECURITY_EMAIL_HTML_TEMPLATE,
                        request.OldEmail,
                        request.NewEmail,
                        supportEmail),
                    To =
                    [
                        new EmailAddressRequest { Email = request.OldEmail }
                    ]
                }
            },
            cancellationToken);

        if (!sent)
        {
            // Notification failure is logged but does not block ownership verification of the new email.
            _logger.LogWarning(InfrastructureLogConstants.EmailLogs.CHANGE_EMAIL_SECURITY_NOTIFICATION_FAILED);
        }

        return sent.Adapt<OtpEmailResponseDto>();
    }

    /// <summary>
    /// Exchanges a refresh token for a new login token pair.
    /// </summary>
    /// <param name="request">The refresh-token request payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The rotated access and refresh token payload.</returns>
    public async Task<LoginResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Reject an empty credential before hashing or entering the session transaction.
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new ApiException(
                ApplicationErrorConstants.AccountErrors.INVALID_REFRESH_TOKEN_MESSAGE,
                ApplicationErrorConstants.TokenErrorCodes.AUTH_INVALID_REFRESH_TOKEN,
                StatusCodes.Status401Unauthorized);
        }

        var refreshTokenHash = AuthSessionHelper.HashRefreshToken(request.RefreshToken);
        var deviceContext = _clientDeviceContextAccessor.GetCurrent();
        User refreshedUser = null;
        RefreshToken refreshedSession = null;

        var response = await _unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                // Step 2: Lock the matching current or previous hash and recheck session eligibility under that lock.
                var existingToken = await _repositories.RefreshTokenRepository.GetForRefreshForUpdateAsync(
                    refreshTokenHash,
                    ct);
                if (!IsRefreshTokenUsable(existingToken))
                {
                    throw CreateInvalidRefreshTokenException();
                }

                // Step 3: An immediate previous hash is a duplicate retry, so reject it without revoking the session.
                if (!string.Equals(existingToken.TokenHash, refreshTokenHash, StringComparison.Ordinal))
                {
                    _logger.LogWarning(
                        InfrastructureLogConstants.SessionLogs.REFRESH_TOKEN_REUSE_REJECTED,
                        existingToken.User.PublicId,
                        existingToken.SessionPublicId);
                    throw new ApiException(
                        ApplicationErrorConstants.AccountErrors.REFRESH_TOKEN_DUPLICATE_MESSAGE,
                        ApplicationErrorConstants.TokenErrorCodes.AUTH_REFRESH_DUPLICATE,
                        StatusCodes.Status409Conflict);
                }

                // Step 4: Rotate the locked current session in place and preserve its public session identifier.
                existingToken.CurrentPartyId = await _repositories.UserPartyRepository.ResolveSessionPartyIdAsync(
                                                   existingToken.UserId,
                                                   existingToken.CurrentPartyId,
                                                   ct)
                                               ?? throw new ApiException(
                                                   ApplicationErrorConstants.ContextErrors.PARTY_CONTEXT_NOT_AVAILABLE_MESSAGE,
                                                   ApplicationErrorConstants.AccountErrorCodes.AUTH_PARTY_CONTEXT_NOT_AVAILABLE);

                refreshedUser = existingToken.User;
                refreshedSession = existingToken;
                return await StageIssueAsync(
                    new AuthSessionIssueRequestModel
                    {
                        User = existingToken.User,
                        RawRefreshToken = AuthSessionHelper.GenerateRefreshToken(),
                        SessionRefreshToken = existingToken,
                        DeviceContext = deviceContext,
                        RenewSessionPublicId = false,
                        AuthOptions = _authOptions,
                        JwtSecurityTokenHandler = _jwtSecurityTokenHandler
                    },
                    ct);
            },
            cancellationToken);

        // Step 5: Publish required auth-reset cache state only after the refresh transaction commits.
        await TrySeedAuthResetMarkerAsync(
            refreshedUser.PublicId,
            refreshedUser.AuthResetAt,
            cancellationToken);
        _logger.LogInformation(
            InfrastructureLogConstants.SessionLogs.CLIENT_SESSION_REFRESHED,
            refreshedSession.SessionPublicId,
            refreshedUser.PublicId);
        return response;
    }

    /// <summary>
    /// Determines whether a locked refresh-token row and its owning account can rotate safely.
    /// </summary>
    /// <param name="refreshToken">The refresh-token row loaded for rotation.</param>
    /// <returns><c>true</c> when the session and account remain eligible; otherwise <c>false</c>.</returns>
    private static bool IsRefreshTokenUsable(RefreshToken refreshToken)
    {
        // Require active session state and an active confirmed account before any hash branch can rotate.
        return refreshToken is not null
               && refreshToken.SessionPublicId.HasValue
               && !refreshToken.RevokedAt.HasValue
               && refreshToken.ExpiresAt > DateTime.UtcNow
               && refreshToken.User is not null
               && !refreshToken.User.IsDeleted
               && refreshToken.User.Status is not null
               && string.Equals(refreshToken.User.Status.Code, ACTIVE_STATUS, StringComparison.OrdinalIgnoreCase)
               && refreshToken.User.EmailConfirmed;
    }

    /// <summary>
    /// Creates the stable unauthorised error returned for unusable refresh credentials.
    /// </summary>
    /// <returns>The invalid-refresh-token API exception.</returns>
    private static ApiException CreateInvalidRefreshTokenException()
    {
        // Centralize the public error contract shared by all invalid refresh states.
        return new ApiException(
            ApplicationErrorConstants.AccountErrors.INVALID_REFRESH_TOKEN_MESSAGE,
            ApplicationErrorConstants.TokenErrorCodes.AUTH_INVALID_REFRESH_TOKEN,
            StatusCodes.Status401Unauthorized);
    }

    /// <summary>
    /// Revokes refresh token state for logout.
    /// </summary>
    /// <param name="currentUserPublicId">The current authenticated user's public identifier.</param>
    /// <param name="currentSessionPublicId">The current authenticated session's public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The logout result.</returns>
    public async Task<OperationStatusResponseDto> LogoutAsync(
        Guid currentUserPublicId,
        Guid currentSessionPublicId,
        CancellationToken cancellationToken = default)
    {
        // Resolve the public user id to the internal user id used by refresh-token rows.
        var user = await _repositories.UserRepository.GetByPublicIdAsync(currentUserPublicId, cancellationToken)
                   ?? throw new HttpStatusCodeException(
                       ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                       UNAUTHORIZED,
                       StatusCodes.Status401Unauthorized);

        // Logout always revokes only the trusted current session from the access token.
        await RevokeSessionAndRefreshTokensAsync(
            user.Id,
            currentSessionPublicId,
            cancellationToken);
        _logger.LogInformation(
            InfrastructureLogConstants.SessionLogs.CLIENT_SESSION_REVOKED,
            currentSessionPublicId,
            currentUserPublicId);

        return OperationStatusResponseHelper.Success(
            ApplicationMessageConstants.SessionMessages.LOGOUT_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Changes a password after forgot-password verification.
    /// </summary>
    /// <param name="request">The forgot-password change-password payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The password-change result.</returns>
    public async Task<OperationStatusResponseDto> ChangeForgotPasswordAsync(
        ChangeForgotPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var authResetAt = DateTime.UtcNow;
        User changedUser = null;
        await _unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                // Step 1: Lock and recheck the reset target before changing credential state.
                var user = await _repositories.UserRepository.GetPasswordIdentityByEmailForUpdateAsync(
                               request.Email,
                               ct)
                           ?? throw new ApiException(
                               ApplicationErrorConstants.OtpErrors.RESET_SESSION_INVALID_MESSAGE,
                               ApplicationErrorConstants.OtpErrorCodes.AUTH_RESET_SESSION_INVALID);

                // Step 2: Advance required auth-reset state, replace the password, and revoke every old session atomically.
                await WriteAuthResetMarkerAsync(user.PublicId, authResetAt, ct);
                await _repositories.UserRepository.StagePasswordHashChangeAsync(
                    user.Id,
                    _passwordHasher.HashPassword(user, request.NewPassword),
                    authResetAt,
                    authResetAt);
                await _repositories.RefreshTokenRepository.StageActiveRevocationsByUserIdAsync(
                    user.Id,
                    authResetAt,
                    ct);
                changedUser = user;
            },
            cancellationToken);
        // Step 3: Seed the committed reset marker before reporting password-reset success.
        await TrySeedAuthResetMarkerAsync(changedUser.PublicId, authResetAt, cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.PasswordLogs.FORGOT_PASSWORD_CHANGED,
            changedUser.PublicId);
        _logger.LogInformation(
            InfrastructureLogConstants.SessionLogs.CLIENT_SESSIONS_REVOKED_BY_CREDENTIAL_CHANGE,
            changedUser.PublicId);

        return OperationStatusResponseHelper.Success(
            ApplicationMessageConstants.PasswordMessages.PASSWORD_CHANGED_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Changes the current authenticated user's password.
    /// </summary>
    /// <param name="currentUserPublicId">The current authenticated user's public identifier.</param>
    /// <param name="currentSessionPublicId">The current authenticated session's public identifier.</param>
    /// <param name="request">The authenticated change-password payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The password-change result.</returns>
    public async Task<ChangePasswordResponseDto> ChangePasswordAsync(
        Guid currentUserPublicId,
        Guid currentSessionPublicId,
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var authResetAt = DateTime.UtcNow;
        User changedUser = null;
        var login = await _unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                // Step 1: Lock the user row so concurrent password changes cannot verify the same old password.
                var user = await _repositories.UserRepository.GetTrackedByPublicIdForUpdateAsync(
                               currentUserPublicId,
                               ct)
                           ?? throw new HttpStatusCodeException(
                               ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                               UNAUTHORIZED,
                               StatusCodes.Status401Unauthorized);
                // Step 2: Verify the submitted current password against the locked, latest credential state.
                if (string.IsNullOrWhiteSpace(user.PasswordHash)
                    || _passwordHasher.VerifyHashedPassword(
                        user,
                        user.PasswordHash,
                        request.CurrentPassword) == PasswordVerificationResult.Failed)
                {
                    throw new ApiException(
                        ApplicationErrorConstants.PasswordErrors.CURRENT_PASSWORD_INVALID_MESSAGE,
                        ApplicationErrorConstants.AccountErrorCodes.AUTH_INVALID_CREDENTIALS);
                }

                // Step 3: Lock the trusted current session before revoking all other device sessions.
                var currentSession = await _repositories.RefreshTokenRepository
                                         .GetByUserAndSessionPublicIdForUpdateAsync(
                                             user.Id,
                                             currentSessionPublicId,
                                             ct)
                                     ?? throw new HttpStatusCodeException(
                                         ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                         UNAUTHORIZED,
                                         StatusCodes.Status401Unauthorized);

                // Step 4: Change the password, revoke other sessions, and rotate the current session in one transaction.
                await WriteAuthResetMarkerAsync(user.PublicId, authResetAt, ct);
                user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
                user.AuthResetAt = authResetAt;
                user.UpdatedAt = authResetAt;
                await _repositories.RefreshTokenRepository.StageActiveRevocationsExceptSessionAsync(
                    user.Id,
                    currentSessionPublicId,
                    authResetAt,
                    ct);

                changedUser = user;
                return await StageIssueAsync(
                    new AuthSessionIssueRequestModel
                    {
                        User = user,
                        RawRefreshToken = AuthSessionHelper.GenerateRefreshToken(),
                        SessionRefreshToken = currentSession,
                        DeviceContext = _clientDeviceContextAccessor.GetCurrent(),
                        RenewSessionPublicId = false,
                        AuthOptions = _authOptions,
                        JwtSecurityTokenHandler = _jwtSecurityTokenHandler
                    },
                    ct);
            },
            cancellationToken);

        // Step 5: Publish the committed auth-reset marker before returning the replacement token pair.
        await TrySeedAuthResetMarkerAsync(changedUser.PublicId, changedUser.AuthResetAt, cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.PasswordLogs.PASSWORD_CHANGED,
            currentUserPublicId);

        return new ChangePasswordResponseDto
        {
            Message = ApplicationMessageConstants.PasswordMessages.PASSWORD_CHANGED_SUCCESS_MESSAGE,
            Login = login
        };
    }

    /// <summary>
    /// Issues a Haven access token and persists the matching client-session refresh token.
    /// </summary>
    /// <param name="request">The grouped auth-session issue request.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The login response containing the access and refresh token pair.</returns>
    private async Task<LoginResponseDto> IssueAsync(
        AuthSessionIssueRequestModel request,
        CancellationToken cancellationToken = default)
    {
        var response = await StageIssueAsync(request, cancellationToken);

        // Commit refresh-token state before publishing the auth-reset marker used to invalidate issued access tokens.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Seed the reset marker after persistence so the cache never represents an uncommitted auth state.
        await TrySeedAuthResetMarkerAsync(request.User.PublicId, request.User.AuthResetAt, cancellationToken);

        return response;
    }

    /// <summary>
    /// Stages access-token response data and the matching refresh-session mutation without committing it.
    /// </summary>
    /// <param name="request">The grouped auth-session issue request.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The staged login response.</returns>
    private async Task<LoginResponseDto> StageIssueAsync(
        AuthSessionIssueRequestModel request,
        CancellationToken cancellationToken)
    {
        // Build the token pair and stage either a new device session or an in-place session rotation.
        var issueModel = AuthSessionHelper.BuildIssueModel(request);

        if (issueModel.RefreshToken.Id == 0)
        {
            await _repositories.RefreshTokenRepository.AddAsync(issueModel.RefreshToken, cancellationToken);
        }
        else
        {
            await _repositories.RefreshTokenRepository.UpdateAsync(issueModel.RefreshToken);
        }

        return issueModel.LoginResponse;
    }

    /// <summary>
    /// Writes the auth reset marker to cache as a required precondition for password resets.
    /// </summary>
    /// <param name="userPublicId">The public user identifier.</param>
    /// <param name="authResetAt">The UTC auth reset timestamp.</param>
    /// <param name="cancellationToken">The token used to cancel the cache write.</param>
    /// <returns>A task that completes when the required cache write succeeds.</returns>
    private async Task WriteAuthResetMarkerAsync(
        Guid userPublicId,
        DateTime authResetAt,
        CancellationToken cancellationToken)
    {
        try
        {
            // Write the cache marker first so old access tokens stop being accepted before persistence completes.
            await _cachingService.SetAbsoluteAsync(
                TokenHelper.BuildAuthResetCacheKey(userPublicId),
                TokenHelper.ToAuthResetUnixMilliseconds(authResetAt),
                TokenHelper.GetAuthResetCacheTtl(_authOptions.RefreshTokenDays),
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, InfrastructureLogConstants.SessionLogs.AUTH_RESET_CACHE_WRITE_FAILED, userPublicId);
            throw new HttpStatusCodeException(
                AUTH_STATE_UNAVAILABLE,
                SERVICE_UNAVAILABLE,
                StatusCodes.Status503ServiceUnavailable);
        }
    }

    /// <summary>
    /// Seeds the auth reset marker after token issue or reset persistence.
    /// </summary>
    /// <param name="userPublicId">The public user identifier whose reset marker should be cached.</param>
    /// <param name="authResetAt">The optional UTC auth reset marker.</param>
    /// <param name="cancellationToken">The token used to cancel the cache write.</param>
    /// <returns>A task that completes after best-effort cache seeding.</returns>
    private async Task TrySeedAuthResetMarkerAsync(
        Guid userPublicId,
        DateTime? authResetAt,
        CancellationToken cancellationToken)
    {
        try
        {
            // Token issue should not fail when only the cache seed is unavailable; handler can fall back to DB later.
            await _cachingService.SetAbsoluteAsync(
                TokenHelper.BuildAuthResetCacheKey(userPublicId),
                TokenHelper.ToAuthResetUnixMilliseconds(authResetAt),
                TokenHelper.GetAuthResetCacheTtl(_authOptions.RefreshTokenDays),
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, InfrastructureLogConstants.SessionLogs.AUTH_RESET_CACHE_SEED_FAILED, userPublicId);
        }
    }

    /// <summary>
    /// Revokes the current client session by its server-issued session id.
    /// </summary>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="sessionPublicId">The current access token session identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that completes when the session revocation is saved.</returns>
    private async Task RevokeSessionAndRefreshTokensAsync(
        long userId,
        Guid sessionPublicId,
        CancellationToken cancellationToken)
    {
        // Current-session logout marks only the active refresh-token row behind the trusted session id.
        var revokedAt = DateTime.UtcNow;

        await _repositories.RefreshTokenRepository.StageActiveRevocationsByUserAndSessionPublicIdAsync(
            userId,
            sessionPublicId,
            revokedAt,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

}
