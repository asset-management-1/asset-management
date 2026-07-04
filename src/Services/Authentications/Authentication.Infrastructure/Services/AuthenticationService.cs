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
        if (!AuthenticationFlowHelper.CanLogin(user)
            || string.IsNullOrWhiteSpace(user.PasswordHash)
            || _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password) == PasswordVerificationResult.Failed)
        {
            throw new ApiException(
                INVALID_USERNAME_OR_PASSWORD_MESSAGE,
                AUTH_INVALID_CREDENTIALS,
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
        _logger.LogInformation(InfrastructureLogConstants.SessionLogs.LOCAL_LOGIN_COMPLETED);

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
    /// <param name="request">The normalized uniqueness values from the registration payload.</param>
    /// <param name="partyType">The normalized party type master-data value.</param>
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
            throw new ApiException(INVALID_PARTY_TYPE_MESSAGE, AUTH_FORBIDDEN_OPERATION);
        }
    }

    /// <summary>
    /// Validates that username, email, and phone number can create a unique account.
    /// </summary>
    /// <param name="request">The normalized uniqueness values from the registration payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that completes when uniqueness checks pass.</returns>
    private async Task EnsureRegistrationUniquenessAsync(
        RegistrationUniquenessRequestDto request,
        CancellationToken cancellationToken)
    {
        // Each uniqueness check maps to a database constraint the registration flow must respect.
        if (await _repositories.UserRepository.UserNameExistsAsync(request.UserName, cancellationToken))
        {
            throw new ApiException(USERNAME_ALREADY_EXISTS_MESSAGE, AUTH_USER_ALREADY_EXISTS);
        }

        if (await _repositories.UserRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new ApiException(EMAIL_ALREADY_EXISTS_MESSAGE, AUTH_USER_ALREADY_EXISTS);
        }

        if (await _repositories.UserRepository.PhoneNumberExistsAsync(request.PhoneNumber, cancellationToken))
        {
            throw new ApiException(PHONE_NUMBER_ALREADY_EXISTS_MESSAGE, AUTH_USER_ALREADY_EXISTS);
        }
    }

    /// <summary>
    /// Determines whether a non-deleted user exists for the supplied email.
    /// </summary>
    /// <param name="normalizedEmail">The normalized email address.</param>
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
                REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                AUTH_USER_STATUS_NOT_FOUND));
        var partyStatus = MasterDataValueHelper.GetRequired(
            masterDataValues,
            new MasterDataValueRequirementModel(
                PARTY_STATUS_TYPE,
                ACTIVE_STATUS,
                REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                AUTH_USER_STATUS_NOT_FOUND));
        var userStatus = MasterDataValueHelper.GetRequired(
            masterDataValues,
            new MasterDataValueRequirementModel(
                USER_STATUS_TYPE,
                ACTIVE_STATUS,
                REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                AUTH_USER_STATUS_NOT_FOUND));

        var provision = pendingRegister.Adapt<RegisterAccountProvisionRequestDto>();
        provision.PartyTypeId = partyType.Id;
        provision.PartyStatusId = partyStatus.Id;
        provision.UserStatusId = userStatus.Id;

        // The created user is captured for a completion log after the transaction succeeds.
        User createdUser = null;

        await _unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                // Register creates the identity graph atomically: Party + User + UserParty.
                var party = provision.Adapt<Party>();
                var user = provision.Adapt<User>();
                user.CurrentParty = party;

                await _repositories.PartyRepository.AddAsync(party, ct);
                await _repositories.UserRepository.AddAsync(user, ct);
                await _repositories.UserPartyRepository.AddAsync(new UserParty { User = user, Party = party }, ct);
                createdUser = user;
            },
            cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.SessionLogs.REGISTER_COMPLETED,
            createdUser?.PublicId);

        return OperationStatusResponseHelper.Success(REGISTRATION_COMPLETED_SUCCESS_MESSAGE);
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
                    Body = string.Format(OTP_EMAIL_HTML_TEMPLATE, request.OtpCode),
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
                    Subject = ApplicationConstants.EMAIL_SUBJECT_CHANGE_EMAIL_SECURITY,
                    Body = string.Format(
                        CHANGE_EMAIL_SECURITY_EMAIL_HTML_TEMPLATE,
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
        // A missing refresh token cannot be rotated into a new session.
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new ApiException(
                INVALID_REFRESH_TOKEN_MESSAGE,
                AUTH_INVALID_REFRESH_TOKEN,
                StatusCodes.Status401Unauthorized);
        }

        // Resolve the active client session and user login state from the hashed refresh token.
        var refreshTokenHash = AuthSessionHelper.HashRefreshToken(request.RefreshToken);
        var existingToken = await _repositories.RefreshTokenRepository.GetForRefreshAsync(
            refreshTokenHash,
            cancellationToken);
        if (existingToken is null
            || !existingToken.SessionPublicId.HasValue
            || existingToken.RevokedAt.HasValue
            || existingToken.ExpiresAt <= DateTime.UtcNow
            || !AuthenticationFlowHelper.CanLogin(existingToken.User))
        {
            throw new ApiException(
                INVALID_REFRESH_TOKEN_MESSAGE,
                AUTH_INVALID_REFRESH_TOKEN,
                StatusCodes.Status401Unauthorized);
        }

        if (!string.Equals(existingToken.TokenHash, refreshTokenHash, StringComparison.Ordinal))
        {
            // Reusing the immediately previous refresh token revokes the current client session and rejects the request.
            existingToken.RevokedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogWarning(
                InfrastructureLogConstants.SessionLogs.REFRESH_TOKEN_REUSE_REJECTED,
                existingToken.User.PublicId,
                existingToken.SessionPublicId);
            throw new ApiException(
                INVALID_REFRESH_TOKEN_MESSAGE,
                AUTH_INVALID_REFRESH_TOKEN,
                StatusCodes.Status401Unauthorized);
        }

        // Refresh rotates the token hash in place and keeps the same server-issued session id.
        var replacementRefreshToken = AuthSessionHelper.GenerateRefreshToken();
        var deviceContext = _clientDeviceContextAccessor.GetCurrent();
        var response = await IssueAsync(
            new AuthSessionIssueRequestModel
            {
                User = existingToken.User,
                RawRefreshToken = replacementRefreshToken,
                SessionRefreshToken = existingToken,
                DeviceContext = deviceContext,
                RenewSessionPublicId = false,
                AuthOptions = _authOptions,
                JwtSecurityTokenHandler = _jwtSecurityTokenHandler
            },
            cancellationToken);
        _logger.LogInformation(
            InfrastructureLogConstants.SessionLogs.CLIENT_SESSION_REFRESHED,
            existingToken.SessionPublicId,
            existingToken.User.PublicId);
        _logger.LogInformation(InfrastructureLogConstants.SessionLogs.REFRESH_TOKEN_ROTATED);

        return response;
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
                       ApplicationConstants.UNAUTHORIZED_REQUEST_MESSAGE,
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

        return OperationStatusResponseHelper.Success(LOGOUT_SUCCESS_MESSAGE);
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
        // Load password state from the verified reset-session email.
        var user = await _repositories.UserRepository.GetPasswordIdentityByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            throw new ApiException(RESET_SESSION_INVALID_MESSAGE, AUTH_RESET_SESSION_INVALID);
        }

        // Forgot-password completion rotates the password and invalidates every existing session.
        var authResetAt = DateTime.UtcNow;
        await ResetPasswordAuthStateAndRevokeClientSessionsAsync(
            user,
            _passwordHasher.HashPassword(user, request.NewPassword),
            authResetAt,
            cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.PasswordLogs.FORGOT_PASSWORD_CHANGED,
            user.PublicId);

        return OperationStatusResponseHelper.Success(PASSWORD_CHANGED_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Changes the current authenticated user's password.
    /// </summary>
    /// <param name="currentUserPublicId">The current authenticated user's public identifier.</param>
    /// <param name="request">The authenticated change-password payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The password-change result.</returns>
    public async Task<OperationStatusResponseDto> ChangePasswordAsync(
        Guid currentUserPublicId,
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Load current password state from the authenticated public user id.
        var user = await _repositories.UserRepository.GetPasswordIdentityByPublicIdAsync(currentUserPublicId, cancellationToken)
                   ?? throw new HttpStatusCodeException(
                       ApplicationConstants.UNAUTHORIZED_REQUEST_MESSAGE,
                       UNAUTHORIZED,
                       StatusCodes.Status401Unauthorized);
        if (string.IsNullOrWhiteSpace(user.PasswordHash)
            || _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.CurrentPassword) == PasswordVerificationResult.Failed)
        {
            throw new ApiException(CURRENT_PASSWORD_INVALID_MESSAGE, AUTH_INVALID_CREDENTIALS);
        }

        // Authenticated password change rotates the password and invalidates every existing session.
        var authResetAt = DateTime.UtcNow;
        await ResetPasswordAuthStateAndRevokeClientSessionsAsync(
            user,
            _passwordHasher.HashPassword(user, request.NewPassword),
            authResetAt,
            cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.PasswordLogs.PASSWORD_CHANGED,
            currentUserPublicId);

        return OperationStatusResponseHelper.Success(PASSWORD_CHANGED_SUCCESS_MESSAGE);
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
        // Prepare the JWT response and refresh token through the shared auth-session helper.
        var issueModel = AuthSessionHelper.BuildIssueModel(request);

        if (issueModel.RefreshToken.Id == 0)
        {
            // First login for this client instance creates the single persistent row.
            await _repositories.RefreshTokenRepository.AddAsync(
                issueModel.RefreshToken,
                cancellationToken);
        }
        else
        {
            // Repeat login or refresh mutates the existing row instead of appending a new one.
            await _repositories.RefreshTokenRepository.UpdateAsync(issueModel.RefreshToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await TrySeedAuthResetMarkerAsync(request.User.PublicId, request.User.AuthResetAt, cancellationToken);

        return issueModel.LoginResponse;
    }

    /// <summary>
    /// Updates password state, resets authentication state, and revokes every active session.
    /// </summary>
    /// <param name="user">The password identity being changed.</param>
    /// <param name="passwordHash">The new password hash to persist.</param>
    /// <param name="authResetAt">The UTC auth reset timestamp.</param>
    /// <param name="cancellationToken">The token used to cancel the reset operation.</param>
    /// <returns>A task that completes when password, reset state, and refresh-token revocations are persisted.</returns>
    private async Task ResetPasswordAuthStateAndRevokeClientSessionsAsync(
        User user,
        string passwordHash,
        DateTime authResetAt,
        CancellationToken cancellationToken)
    {
        // Write Redis first so already-issued access tokens are invalidated before password persistence.
        await WriteAuthResetMarkerAsync(user.PublicId, authResetAt, cancellationToken);

        // Stage password and reset timestamp together so access-token validity follows credential changes.
        await _repositories.UserRepository.StagePasswordHashChangeAsync(
            user.Id,
            passwordHash,
            authResetAt,
            authResetAt);

        // Revoke every client session in the same save boundary as the password change.
        await _repositories.RefreshTokenRepository.StageActiveRevocationsByUserIdAsync(
            user.Id,
            authResetAt,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            InfrastructureLogConstants.SessionLogs.CLIENT_SESSIONS_REVOKED_BY_CREDENTIAL_CHANGE,
            user.PublicId);

        // Seed Redis again after the DB write so handler fast-path stays aligned with source of truth.
        await TrySeedAuthResetMarkerAsync(user.PublicId, authResetAt, cancellationToken);
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
            // Write Redis first so old access tokens stop being accepted before persistence completes.
            await AuthResetCacheHelper.SetAsync(
                _cachingService,
                new AuthResetCacheWriteModel
                {
                    UserPublicId = userPublicId,
                    AuthResetAt = authResetAt,
                    RefreshTokenDays = _authOptions.RefreshTokenDays
                },
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
            await AuthResetCacheHelper.SetAsync(
                _cachingService,
                new AuthResetCacheWriteModel
                {
                    UserPublicId = userPublicId,
                    AuthResetAt = authResetAt,
                    RefreshTokenDays = _authOptions.RefreshTokenDays
                },
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
