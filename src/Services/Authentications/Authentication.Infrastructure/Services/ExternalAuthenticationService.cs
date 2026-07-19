namespace Authentication.Infrastructure.Services;

/// <summary>
/// Provides infrastructure operations for external authentication flows.
/// </summary>
public class ExternalAuthenticationService : IExternalAuthenticationService
{
    private readonly AuthenticationRepositoryDependencies _repositories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ExternalAuthenticationOptions _externalAuthenticationOptions;
    private readonly AuthOptions _authOptions;
    private readonly ILogger<ExternalAuthenticationService> _logger;
    private readonly IClientDeviceContextAccessor _clientDeviceContextAccessor;
    private readonly IExternalIdentityProviderService _externalIdentityProviderService;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();

    /// <summary>
    /// Creates the external authentication service with provider validation and account-link persistence dependencies.
    /// </summary>
    /// <param name="repositories">The grouped authentication repositories used by persistence operations.</param>
    /// <param name="unitOfWork">The unit of work used for transactional writes.</param>
    /// <param name="externalAuthenticationOptions">The configured external identity-provider options.</param>
    /// <param name="authOptions">The configured JWT and refresh-token options.</param>
    /// <param name="logger">The service logger.</param>
    /// <param name="clientDeviceContextAccessor">The accessor used to capture device metadata for token issuance.</param>
    /// <param name="externalIdentityProviderService">The trust boundary used to validate provider credentials.</param>
    public ExternalAuthenticationService(
        AuthenticationRepositoryDependencies repositories,
        IUnitOfWork unitOfWork,
        IOptions<ExternalAuthenticationOptions> externalAuthenticationOptions,
        IOptions<AuthOptions> authOptions,
        ILogger<ExternalAuthenticationService> logger,
        IClientDeviceContextAccessor clientDeviceContextAccessor,
        IExternalIdentityProviderService externalIdentityProviderService)
    {
        // Store external-auth dependencies while keeping the constructor inside the analyzer limit.
        _repositories = repositories;
        _unitOfWork = unitOfWork;
        _externalAuthenticationOptions = externalAuthenticationOptions.Value;
        _authOptions = authOptions.Value;
        _logger = logger;
        _clientDeviceContextAccessor = clientDeviceContextAccessor;
        _externalIdentityProviderService = externalIdentityProviderService;
    }

    /// <summary>
    /// Authenticates with an external provider.
    /// </summary>
    /// <param name="request">The external-login request payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The existing-account login payload or first-time registration prefill.</returns>
    public async Task<ExternalLoginResponseDto> LoginAsync(
        ExternalLoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Resolve configured provider metadata and validate the provider-owned identity.
        var provider = GetProvider(request.Provider);

        if (provider is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.ExternalProviderErrors.INVALID_EXTERNAL_PROVIDER_MESSAGE,
                ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_PROVIDER_INVALID);
        }

        var profile = await _externalIdentityProviderService.GetValidatedIdentityAsync(
            provider.Name,
            request.ExternalToken,
            cancellationToken);

        // Step 2: Prefer an existing provider mapping after rechecking that its local account can sign in.
        var existingMapping = await _repositories.ExternalLoginRepository.GetByProviderAsync(
            provider.Name,
            profile.ProviderUserId,
            cancellationToken);
        if (existingMapping is not null)
        {
            EnsureLoginEligible(existingMapping.User);

            await _repositories.UserRepository.StageLastLoginAtAsync(existingMapping.UserId, DateTime.UtcNow);

            var response = await IssueAsync(
                existingMapping.User,
                AuthSessionHelper.GenerateRefreshToken(),
                cancellationToken);
            _logger.LogInformation(
                InfrastructureLogConstants.ExternalProviderLogs.EXTERNAL_LOGIN_COMPLETED,
                provider.Name,
                existingMapping.User.PublicId);

            return response.Adapt<ExternalLoginResponseDto>();
        }

        // Step 3: Auto-link only an authoritative provider email to an eligible confirmed Haven account.
        var normalizedEmail = profile.Email.NormalizeEmail();
        var localUser = await _repositories.UserRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (localUser is not null && profile.CanAutoLinkByEmail)
        {
            var linkTarget = await _repositories.UserRepository.GetUserForAuthenticationByIdAsync(
                                 localUser.Id,
                                 cancellationToken)
                             ?? throw new ApiException(
                                 ApplicationErrorConstants.AccountErrors.ACCOUNT_INACTIVE_MESSAGE,
                                 ApplicationErrorConstants.AccountErrorCodes.AUTH_ACCOUNT_INACTIVE,
                                 StatusCodes.Status403Forbidden);
            if (linkTarget.IsDeleted
                || linkTarget.Status is null
                || !string.Equals(linkTarget.Status.Code, ACTIVE_STATUS, StringComparison.OrdinalIgnoreCase)
                || !linkTarget.EmailConfirmed)
            {
                throw new ApiException(
                    ApplicationErrorConstants.AccountErrors.ACCOUNT_INACTIVE_MESSAGE,
                    ApplicationErrorConstants.AccountErrorCodes.AUTH_ACCOUNT_INACTIVE,
                    StatusCodes.Status403Forbidden);
            }

            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(
                    async ct =>
                    {
                        await _repositories.ExternalLoginRepository.AddAsync(
                            new ExternalLogin
                            {
                                UserId = localUser.Id,
                                LoginProvider = provider.Name,
                                ProviderKey = profile.ProviderUserId
                            },
                            ct);
                    },
                    cancellationToken);
            }
            catch (DbUpdateException exception)
                when (PostgreSqlExceptionHelper.GetUniqueConstraintName(exception)
                    is EXTERNAL_PROVIDER_KEY_UNIQUE_CONSTRAINT or EXTERNAL_USER_PROVIDER_UNIQUE_CONSTRAINT)
            {
                _unitOfWork.ClearTrackedChanges();
                var winner = await _repositories.ExternalLoginRepository.GetByProviderAsync(
                    provider.Name,
                    profile.ProviderUserId,
                    cancellationToken);
                if (winner?.UserId != localUser.Id)
                {
                    throw new ApiException(
                        ApplicationErrorConstants.ExternalProviderErrors.EXTERNAL_PROVIDER_LINK_CONFLICT_MESSAGE,
                        ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_PROVIDER_LINK_CONFLICT,
                        StatusCodes.Status409Conflict);
                }
            }

            var login = await IssueAsync(
                linkTarget,
                AuthSessionHelper.GenerateRefreshToken(),
                cancellationToken);
            return login.Adapt<ExternalLoginResponseDto>();
        }

        // Step 4: A first-time identity returns provider prefill only and creates no local or cache state.
        return new ExternalLoginResponseDto
        {
            IsNewRegistration = true,
            Registration = profile.Adapt<ExternalRegistrationPrefillDto>()
        };
    }

    /// <summary>
    /// Revalidates a first-time provider identity and completes the local account graph atomically.
    /// </summary>
    /// <param name="request">The validated external-registration fields and provider credential.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The issued token pair for the completed or concurrently-created account.</returns>
    public async Task<LoginResponseDto> CompleteRegistrationAsync(
        CompleteExternalRegistrationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Revalidate provider-owned identity; client fields never establish email or provider ownership.
        var provider = GetProvider(request.Provider)
                       ?? throw new ApiException(
                           ApplicationErrorConstants.ExternalProviderErrors.INVALID_EXTERNAL_PROVIDER_MESSAGE,
                           ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_PROVIDER_INVALID);
        var profile = await _externalIdentityProviderService.GetValidatedIdentityAsync(
            provider.Name,
            request.ExternalToken,
            cancellationToken);
        // Step 2: Recheck provider mapping and local email before attempting account creation.
        var existingMapping = await _repositories.ExternalLoginRepository.GetByProviderAsync(
            provider.Name,
            profile.ProviderUserId,
            cancellationToken);
        if (existingMapping is not null)
        {
            // A completed concurrent registration may log in only when the winner account remains eligible.
            EnsureLoginEligible(existingMapping.User);
            return await IssueAsync(
                existingMapping.User,
                AuthSessionHelper.GenerateRefreshToken(),
                cancellationToken);
        }

        var normalizedEmail = profile.Email.NormalizeEmail();
        if (await _repositories.UserRepository.GetByEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            throw new ApiException(
                ApplicationErrorConstants.AccountErrors.ACCOUNT_ALREADY_EXISTS_LINK_MESSAGE,
                ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_PROVIDER_LINK_CONFLICT,
                StatusCodes.Status409Conflict);
        }

        // Step 3: Create the full local identity graph once; database uniqueness remains the final concurrency boundary.
        User createdUser;
        try
        {
            createdUser = await CreateExternalUserAsync(
                provider,
                profile,
                request,
                cancellationToken);
        }
        catch (DbUpdateException exception)
            when (PostgreSqlExceptionHelper.GetUniqueConstraintName(exception)
                is EXTERNAL_PROVIDER_KEY_UNIQUE_CONSTRAINT
                or EXTERNAL_USER_PROVIDER_UNIQUE_CONSTRAINT
                or USER_EMAIL_UNIQUE_CONSTRAINT
                or USER_PHONE_UNIQUE_CONSTRAINT
                or USER_NAME_UNIQUE_CONSTRAINT)
        {
            // Clear failed Added entities before loading the concurrent winner and staging its device session.
            _unitOfWork.ClearTrackedChanges();
            var winner = await _repositories.ExternalLoginRepository.GetByProviderAsync(
                provider.Name,
                profile.ProviderUserId,
                cancellationToken);
            if (winner?.User is null)
            {
                throw new ApiException(
                    ApplicationErrorConstants.ExternalProviderErrors.EXTERNAL_PROVIDER_LINK_CONFLICT_MESSAGE,
                    ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_PROVIDER_LINK_CONFLICT,
                    StatusCodes.Status409Conflict);
            }

            EnsureLoginEligible(winner.User);

            return await IssueAsync(
                winner.User,
                AuthSessionHelper.GenerateRefreshToken(),
                cancellationToken);
        }
        // Step 4: Issue the device session only after the account graph transaction commits successfully.
        return await IssueAsync(
            createdUser,
            AuthSessionHelper.GenerateRefreshToken(),
            cancellationToken);
    }

    /// <summary>
    /// Links an external provider to the current user.
    /// </summary>
    /// <param name="request">The external-link request payload.</param>
    /// <param name="currentUserPublicId">The current authenticated user's public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The external-link result.</returns>
    public async Task<OperationStatusResponseDto> LinkAsync(
        LinkExternalProviderRequestDto request,
        Guid currentUserPublicId,
        CancellationToken cancellationToken = default)
    {
        // Load the current user row required to own the new provider mapping.
        var user = await _repositories.UserRepository.GetByPublicIdAsync(currentUserPublicId, cancellationToken)
                   ?? throw new HttpStatusCodeException(
                       ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                       UNAUTHORIZED,
                       StatusCodes.Status401Unauthorized);

        // Resolve provider configuration before trusting the submitted external token.
        var provider = GetProvider(request.Provider);

        if (provider is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.ExternalProviderErrors.INVALID_EXTERNAL_PROVIDER_MESSAGE,
                ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_PROVIDER_INVALID);
        }

        // Validate the external token and extract the provider user id used for linking.
        var profile = await _externalIdentityProviderService.GetValidatedIdentityAsync(
            provider.Name,
            request.ExternalToken,
            cancellationToken);

        // Existing links are idempotent only when they point to the same provider user id.
        var existingUserProvider = await _repositories.ExternalLoginRepository.GetByUserAndProviderAsync(
            user.Id,
            provider.Name,
            cancellationToken);
        if (existingUserProvider is not null)
        {
            if (string.Equals(existingUserProvider.ProviderKey, profile.ProviderUserId, StringComparison.Ordinal))
            {
                _logger.LogInformation(
                    InfrastructureLogConstants.ExternalProviderLogs.PROVIDER_LINKED,
                    provider.Name,
                    currentUserPublicId);
                return OperationStatusResponseHelper.Success(
                    ApplicationMessageConstants.ExternalProviderMessages.EXTERNAL_PROVIDER_LINKED_SUCCESS_MESSAGE);
            }

            throw new ApiException(
                ApplicationErrorConstants.ExternalProviderErrors.EXTERNAL_PROVIDER_LINK_CONFLICT_MESSAGE,
                ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_PROVIDER_LINK_CONFLICT);
        }

        // Check soft-deleted local links and active global mappings before creating or reviving a link.
        var deletedUserProvider = await _repositories.ExternalLoginRepository.GetDeletedByUserAndProviderAsync(
            user.Id,
            provider.Name,
            cancellationToken);
        var existingMapping = await _repositories.ExternalLoginRepository.GetByProviderAsync(
            provider.Name,
            profile.ProviderUserId,
            cancellationToken);
        if (existingMapping is not null && existingMapping.UserId != user.Id)
        {
            throw new ApiException(
                ApplicationErrorConstants.ExternalProviderErrors.EXTERNAL_PROVIDER_LINK_CONFLICT_MESSAGE,
                ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_PROVIDER_LINK_CONFLICT);
        }

        try
        {
            if (deletedUserProvider is not null)
            {
                // Step 1: Revive the user's previous provider row so relinking stays idempotent.
                deletedUserProvider.ProviderKey = profile.ProviderUserId;
                deletedUserProvider.IsDeleted = false;
                await _repositories.ExternalLoginRepository.UpdateAsync(deletedUserProvider);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    InfrastructureLogConstants.ExternalProviderLogs.PROVIDER_LINKED,
                    provider.Name,
                    currentUserPublicId);

                return OperationStatusResponseHelper.Success(
                    ApplicationMessageConstants.ExternalProviderMessages.EXTERNAL_PROVIDER_LINKED_SUCCESS_MESSAGE);
            }

            if (existingMapping is null)
            {
                // Step 2: Create the provider row when no active identity mapping exists.
                await _repositories.ExternalLoginRepository.AddAsync(
                    new ExternalLogin
                    {
                        UserId = user.Id,
                        LoginProvider = provider.Name,
                        ProviderKey = profile.ProviderUserId
                    },
                    cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        catch (DbUpdateException exception)
            when (PostgreSqlExceptionHelper.GetUniqueConstraintName(exception)
                is EXTERNAL_PROVIDER_KEY_UNIQUE_CONSTRAINT or EXTERNAL_USER_PROVIDER_UNIQUE_CONSTRAINT)
        {
            // Step 3: A concurrent link winner returns the stable provider-conflict contract.
            throw new ApiException(
                ApplicationErrorConstants.ExternalProviderErrors.EXTERNAL_PROVIDER_LINK_CONFLICT_MESSAGE,
                ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_PROVIDER_LINK_CONFLICT);
        }

        _logger.LogInformation(
            InfrastructureLogConstants.ExternalProviderLogs.PROVIDER_LINKED,
            provider.Name,
            currentUserPublicId);

        return OperationStatusResponseHelper.Success(
            ApplicationMessageConstants.ExternalProviderMessages.EXTERNAL_PROVIDER_LINKED_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Unlinks an external provider from the current user.
    /// </summary>
    /// <param name="request">The external-unlink request payload.</param>
    /// <param name="currentUserPublicId">The current authenticated user's public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The external-unlink result.</returns>
    public async Task<OperationStatusResponseDto> UnlinkAsync(
        UnlinkExternalProviderRequestDto request,
        Guid currentUserPublicId,
        CancellationToken cancellationToken = default)
    {
        // Load active provider links with the user so final sign-in-method rules can be enforced.
        var user = await _repositories.UserRepository.GetByPublicIdWithExternalLoginsAsync(
            currentUserPublicId,
            cancellationToken)
                   ?? throw new HttpStatusCodeException(
                       ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                       UNAUTHORIZED,
                       StatusCodes.Status401Unauthorized);

        // Resolve provider configuration before locating the link to remove.
        var provider = GetProvider(request.Provider);

        if (provider is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.ExternalProviderErrors.INVALID_EXTERNAL_PROVIDER_MESSAGE,
                ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_PROVIDER_INVALID);
        }

        // Only the current user's active provider link can be unlinked.
        var externalLogin = await _repositories.ExternalLoginRepository.GetByUserAndProviderAsync(
            user.Id,
            provider.Name,
            cancellationToken);
        if (externalLogin is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.ExternalProviderErrors.EXTERNAL_PROVIDER_NOT_LINKED_MESSAGE,
                ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_PROVIDER_NOT_LINKED);
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash) && user.ExternalLogins.Count <= 1)
        {
            // Keep at least one usable sign-in method so unlink cannot lock the account out.
            throw new ApiException(
                ApplicationErrorConstants.ExternalProviderErrors.LAST_SIGN_IN_METHOD_REQUIRED_MESSAGE,
                ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_LAST_SIGN_IN_METHOD_REQUIRED);
        }

        // Soft-delete the provider mapping so it can be revived by a later link operation.
        externalLogin.IsDeleted = true;
        await _repositories.ExternalLoginRepository.UpdateAsync(externalLogin);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            InfrastructureLogConstants.ExternalProviderLogs.PROVIDER_UNLINKED,
            provider.Name,
            currentUserPublicId);

        return OperationStatusResponseHelper.Success(
            ApplicationMessageConstants.ExternalProviderMessages.EXTERNAL_PROVIDER_UNLINKED_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Resolves and normalises an external provider descriptor from configured options.
    /// </summary>
    /// <param name="provider">The provider name supplied by the caller.</param>
    /// <returns>The normalised provider descriptor, or <c>null</c> when unsupported.</returns>
    private ExternalProviderDescriptorDto GetProvider(string provider)
    {
        // Read raw options first so default party-type configuration can be normalised for persistence.
        var options = GetProviderOptions(provider);

        if (options is null)
        {
            return null;
        }

        return new ExternalProviderDescriptorDto
        {
            Name = options.Name,
            DefaultPartyType = options.DefaultPartyType.ToMasterDataCode(),
            RequireVerifiedEmail = options.RequireVerifiedEmail
        };
    }

    /// <summary>
    /// Creates a new Haven account from a validated external identity profile.
    /// </summary>
    /// <param name="provider">The resolved external provider descriptor.</param>
    /// <param name="profile">The validated external identity profile.</param>
    /// <param name="request">The validated local registration fields supplied by the client.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The created user reloaded with authentication graph data.</returns>
    private async Task<User> CreateExternalUserAsync(
        ExternalProviderDescriptorDto provider,
        ExternalIdentityProfileResponseDto profile,
        CompleteExternalRegistrationRequestDto request,
        CancellationToken cancellationToken)
    {
        // Resolve account creation statuses and default party type in one master-data batch.
        var masterDataValues = await _repositories.MasterDataValueRepository.GetByTypeAndValuesAsync(
            [
                new MasterDataValueKeyModel(PARTY_STATUS_TYPE, ACTIVE_STATUS),
                new MasterDataValueKeyModel(USER_STATUS_TYPE, ACTIVE_STATUS),
                new MasterDataValueKeyModel(PARTY_TYPE_TYPE, request.PartyType)
            ],
            cancellationToken);
        var partyStatus = MasterDataValueHelper.GetRequired(
            masterDataValues,
            new MasterDataValueRequirementModel(
                PARTY_STATUS_TYPE,
                ACTIVE_STATUS,
                ApplicationErrorConstants.ContextErrors.ACTIVE_STATUS_NOT_FOUND_MESSAGE));
        var userStatus = MasterDataValueHelper.GetRequired(
            masterDataValues,
            new MasterDataValueRequirementModel(
                USER_STATUS_TYPE,
                ACTIVE_STATUS,
                ApplicationErrorConstants.ContextErrors.ACTIVE_STATUS_NOT_FOUND_MESSAGE));
        var partyType = MasterDataValueHelper.GetRequired(
            masterDataValues,
            new MasterDataValueRequirementModel(
                PARTY_TYPE_TYPE,
                request.PartyType,
                ApplicationErrorConstants.ContextErrors.DEFAULT_PARTY_TYPE_NOT_FOUND_MESSAGE));
        var fullName = request.FullName.Trim();
        var normalizedEmail = profile.Email.NormalizeEmail();
        var provision = new ExternalAccountProvisionRequestDto
        {
            PartyTypeId = partyType.Id,
            PartyStatusId = partyStatus.Id,
            UserStatusId = userStatus.Id,
            UserName = await GenerateUniqueUserNameAsync(normalizedEmail, fullName, cancellationToken),
            Email = normalizedEmail,
            PhoneNumber = request.PhoneNumber,
            FullName = fullName,
            EmailConfirmed = profile.EmailVerified
        };

        // External account provisioning must create all identity rows atomically.
        return await _unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                // External provisioning creates the identity graph atomically:
                // Party + User + UserParty + ExternalLogin.
                var party = provision.Adapt<Party>();
                var user = provision.Adapt<User>();
                user.LastLoginAt = DateTime.UtcNow;

                await _repositories.PartyRepository.AddAsync(party, ct);
                await _repositories.UserRepository.AddAsync(user, ct);
                await _repositories.UserPartyRepository.AddAsync(new UserParty { User = user, Party = party }, ct);
                await _repositories.ExternalLoginRepository.AddAsync(
                    new ExternalLogin
                    {
                        User = user,
                        LoginProvider = provider.Name,
                        ProviderKey = profile.ProviderUserId
                    },
                    ct);
                await _unitOfWork.SaveChangesAsync(ct);

                return await _repositories.UserRepository.GetUserForAuthenticationByIdAsync(user.Id, ct)
                       ?? throw new HttpStatusCodeException(
                           ApplicationErrorConstants.AccountErrors.EXTERNAL_USER_NOT_LOADED_MESSAGE,
                           StatusCodes.Status500InternalServerError);
            },
            cancellationToken);
    }

    /// <summary>
    /// Generates a unique local username for an externally provisioned account.
    /// </summary>
    /// <param name="email">The normalised external email address, when available.</param>
    /// <param name="fullName">The external display name fallback.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A username that is not currently used by another account.</returns>
    private async Task<string> GenerateUniqueUserNameAsync(
        string email,
        string fullName,
        CancellationToken cancellationToken)
    {
        // Prefer the email prefix, then display name, and finally a stable external-user fallback.
        var baseName = !string.IsNullOrWhiteSpace(email)
            ? email.Split('@')[0]
            : fullName.Replace(" ", string.Empty, StringComparison.Ordinal);
        baseName = baseName.Trim();
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = DEFAULT_EXTERNAL_USER_NAME;
        }

        var candidate = baseName;
        var suffix = 1;

        while (await _repositories.UserRepository.UserNameExistsAsync(candidate, cancellationToken))
        {
            // Append an incrementing suffix until the generated username is unique.
            suffix++;
            candidate = $"{baseName}{suffix}";
        }

        return candidate;
    }

    /// <summary>
    /// Issues a Haven access token and persists the matching refresh token.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="rawRefreshToken">The raw refresh token returned to the client.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The login response containing the access and refresh token pair.</returns>
    private async Task<LoginResponseDto> IssueAsync(
        User user,
        string rawRefreshToken,
        CancellationToken cancellationToken = default)
    {
        // Prepare the JWT response and refresh token through the shared auth-session helper.
        var deviceContext = _clientDeviceContextAccessor.GetCurrent();
        var sessionRefreshToken = await _repositories.RefreshTokenRepository.GetClientSessionByUserAndDeviceIdAsync(
            user.Id,
            deviceContext.DeviceId,
            cancellationToken);
        var isNewClientSession = sessionRefreshToken is null;
        sessionRefreshToken ??= new RefreshToken();

        // Resolve the Party context independently for this external-login client session.
        sessionRefreshToken.CurrentPartyId = await _repositories.UserPartyRepository.ResolveSessionPartyIdAsync(
                                                 user.Id,
                                                 sessionRefreshToken.CurrentPartyId,
                                                 cancellationToken)
                                             ?? throw new ApiException(
                                                 ApplicationErrorConstants.ContextErrors.PARTY_CONTEXT_NOT_AVAILABLE_MESSAGE,
                                                 ApplicationErrorConstants.AccountErrorCodes.AUTH_PARTY_CONTEXT_NOT_AVAILABLE);
        var issueModel = AuthSessionHelper.BuildIssueModel(
            new AuthSessionIssueRequestModel
            {
                User = user,
                RawRefreshToken = rawRefreshToken,
                SessionRefreshToken = sessionRefreshToken,
                DeviceContext = deviceContext,
                RenewSessionPublicId = true,
                AuthOptions = _authOptions,
                JwtSecurityTokenHandler = _jwtSecurityTokenHandler
            });

        if (issueModel.RefreshToken.Id == 0)
        {
            // First external login for this client instance creates the single persistent row.
            await _repositories.RefreshTokenRepository.AddAsync(
                issueModel.RefreshToken,
                cancellationToken);
        }
        else
        {
            // Repeat external login mutates the existing row instead of appending a new one.
            await _repositories.RefreshTokenRepository.UpdateAsync(issueModel.RefreshToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            isNewClientSession
                ? InfrastructureLogConstants.SessionLogs.CLIENT_SESSION_CREATED
                : InfrastructureLogConstants.SessionLogs.CLIENT_SESSION_REPLACED_BY_LOGIN,
            sessionRefreshToken.SessionPublicId,
            user.PublicId);

        return issueModel.LoginResponse;
    }

    /// <summary>
    /// Gets the raw configured options for an external provider.
    /// </summary>
    /// <param name="provider">The provider name supplied by the caller.</param>
    /// <returns>The configured provider options, or <c>null</c> when unsupported.</returns>
    private ExternalProviderOptions GetProviderOptions(string provider)
    {
        // Application validators and startup validation enforce the same canonical provider codes before this lookup.
        return _externalAuthenticationOptions.Providers.FirstOrDefault(x => x.Name == provider);
    }

    /// <summary>
    /// Ensures an externally resolved Haven account is active, confirmed, and eligible to sign in.
    /// </summary>
    /// <param name="user">The Haven user resolved from an external identity mapping.</param>
    private static void EnsureLoginEligible(User user)
    {
        // Reject deleted, inactive, incomplete, or unconfirmed accounts before issuing any Haven session.
        if (user is null
            || user.IsDeleted
            || user.Status is null
            || !string.Equals(user.Status.Code, ACTIVE_STATUS, StringComparison.OrdinalIgnoreCase)
            || !user.EmailConfirmed)
        {
            throw new ApiException(
                ApplicationErrorConstants.AccountErrors.ACCOUNT_INACTIVE_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_ACCOUNT_INACTIVE,
                StatusCodes.Status403Forbidden);
        }
    }

}
