namespace Authentication.Infrastructure.Services;

/// <summary>
/// Provides infrastructure operations for external authentication flows.
/// </summary>
public class ExternalAuthenticationService : IExternalAuthenticationService
{
    private readonly AuthenticationRepositoryDependencies _repositories;
    private readonly IExternalLoginRepository _externalLoginRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ExternalAuthenticationOptions _externalAuthenticationOptions;
    private readonly AuthOptions _authOptions;
    private readonly ILogger<ExternalAuthenticationService> _logger;
    private readonly IClientDeviceContextAccessor _clientDeviceContextAccessor;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();

    /// <summary>
    /// Creates the external authentication service with provider validation and account-link persistence dependencies.
    /// </summary>
    /// <param name="repositories">The grouped authentication repositories used by persistence operations.</param>
    /// <param name="externalLoginRepository">The external-login repository used for provider mappings.</param>
    /// <param name="unitOfWork">The unit of work used for transactional writes.</param>
    /// <param name="externalAuthenticationOptions">The configured external identity-provider options.</param>
    /// <param name="authOptions">The configured JWT and refresh-token options.</param>
    /// <param name="logger">The service logger.</param>
    /// <param name="clientDeviceContextAccessor">The accessor used to capture device metadata for token issuance.</param>
    public ExternalAuthenticationService(
        AuthenticationRepositoryDependencies repositories,
        IExternalLoginRepository externalLoginRepository,
        IUnitOfWork unitOfWork,
        IOptions<ExternalAuthenticationOptions> externalAuthenticationOptions,
        IOptions<AuthOptions> authOptions,
        ILogger<ExternalAuthenticationService> logger,
        IClientDeviceContextAccessor clientDeviceContextAccessor)
    {
        // Store external-auth dependencies while keeping the constructor inside the analyzer limit.
        _repositories = repositories;
        _externalLoginRepository = externalLoginRepository;
        _unitOfWork = unitOfWork;
        _externalAuthenticationOptions = externalAuthenticationOptions.Value;
        _authOptions = authOptions.Value;
        _logger = logger;
        _clientDeviceContextAccessor = clientDeviceContextAccessor;
    }

    /// <summary>
    /// Returns all configured external provider names.
    /// </summary>
    /// <returns>The supported external provider names.</returns>
    public IReadOnlyCollection<string> GetSupportedProviderNames()
    {
        // Provider names come from configuration and are exposed for validator allow-lists.
        return _externalAuthenticationOptions.Providers
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => x.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Authenticates with an external provider.
    /// </summary>
    /// <param name="request">The external-login request payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The issued Haven login token payload.</returns>
    public async Task<LoginResponseDto> LoginAsync(
        ExternalLoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Resolve configured provider metadata before validating the external token.
        var provider = GetProvider(request.Provider);
        if (provider is null)
        {
            throw new ApiException(INVALID_EXTERNAL_PROVIDER_MESSAGE, AUTH_EXTERNAL_PROVIDER_INVALID);
        }

        // Validate the provider token and map it into a normalized external identity profile.
        var profile = await ValidateAsync(provider.Name, request.ExternalToken, cancellationToken);
        if (profile is null)
        {
            throw new ApiException(INVALID_EXTERNAL_TOKEN_MESSAGE, AUTH_EXTERNAL_PROVIDER_INVALID);
        }

        // Existing provider mappings log in directly after account state is verified.
        var existingMapping = await _externalLoginRepository.GetByProviderAsync(
            provider.Name,
            profile.ProviderUserId,
            cancellationToken);
        if (existingMapping is not null)
        {
            if (!AuthenticationFlowHelper.CanLogin(existingMapping.User))
            {
                throw new ApiException(
                    ACCOUNT_INACTIVE_MESSAGE,
                    AUTH_ACCOUNT_INACTIVE,
                    StatusCodes.Status403Forbidden);
            }

            await _repositories.UserRepository.StageLastLoginAtAsync(existingMapping.UserId, DateTime.UtcNow);

            var response = await IssueAsync(
                existingMapping.User,
                AuthSessionHelper.GenerateRefreshToken(),
                cancellationToken);
            _logger.LogInformation(
                InfrastructureLogConstants.ExternalProviderLogs.EXTERNAL_LOGIN_COMPLETED,
                provider.Name,
                existingMapping.User.PublicId);

            return response;
        }

        // New external accounts cannot auto-provision over an existing local email account.
        var normalizedEmail = profile.Email.NormalizeEmail();
        if (!string.IsNullOrWhiteSpace(normalizedEmail)
            && await _repositories.UserRepository.GetByEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            throw new ApiException(ACCOUNT_ALREADY_EXISTS_LINK_MESSAGE, AUTH_EXTERNAL_PROVIDER_LINK_CONFLICT);
        }

        // Provision a missing external account and issue the first Haven session.
        var createdUser = await CreateExternalUserAsync(provider, profile, cancellationToken);

        var createdResponse = await IssueAsync(
            createdUser,
            AuthSessionHelper.GenerateRefreshToken(),
            cancellationToken);
        _logger.LogInformation(
            InfrastructureLogConstants.ExternalProviderLogs.EXTERNAL_LOGIN_COMPLETED,
            provider.Name,
            createdUser.PublicId);

        return createdResponse;
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
                       ApplicationConstants.UNAUTHORIZED_REQUEST_MESSAGE,
                       UNAUTHORIZED,
                       StatusCodes.Status401Unauthorized);

        // Resolve provider configuration before trusting the submitted external token.
        var provider = GetProvider(request.Provider);
        if (provider is null)
        {
            throw new ApiException(INVALID_EXTERNAL_PROVIDER_MESSAGE, AUTH_EXTERNAL_PROVIDER_INVALID);
        }

        // Validate the external token and extract the provider user id used for linking.
        var profile = await ValidateAsync(provider.Name, request.ExternalToken, cancellationToken);
        if (profile is null)
        {
            throw new ApiException(INVALID_EXTERNAL_TOKEN_MESSAGE, AUTH_EXTERNAL_PROVIDER_INVALID);
        }

        // Existing links are idempotent only when they point to the same provider user id.
        var existingUserProvider = await _externalLoginRepository.GetByUserAndProviderAsync(
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
                return OperationStatusResponseHelper.Success(EXTERNAL_PROVIDER_LINKED_SUCCESS_MESSAGE);
            }

            throw new ApiException(EXTERNAL_PROVIDER_LINK_CONFLICT_MESSAGE, AUTH_EXTERNAL_PROVIDER_LINK_CONFLICT);
        }

        // Check soft-deleted local links and active global mappings before creating or reviving a link.
        var deletedUserProvider = await _externalLoginRepository.GetDeletedByUserAndProviderAsync(
            user.Id,
            provider.Name,
            cancellationToken);
        var existingMapping = await _externalLoginRepository.GetByProviderAsync(
            provider.Name,
            profile.ProviderUserId,
            cancellationToken);
        if (existingMapping is not null && existingMapping.UserId != user.Id)
        {
            throw new ApiException(EXTERNAL_PROVIDER_LINK_CONFLICT_MESSAGE, AUTH_EXTERNAL_PROVIDER_LINK_CONFLICT);
        }

        if (deletedUserProvider is not null)
        {
            // Revive the user's previous soft-deleted provider link so re-link stays idempotent.
            deletedUserProvider.ProviderKey = profile.ProviderUserId;
            deletedUserProvider.IsDeleted = false;
            await _externalLoginRepository.UpdateAsync(deletedUserProvider);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                InfrastructureLogConstants.ExternalProviderLogs.PROVIDER_LINKED,
                provider.Name,
                currentUserPublicId);

            return OperationStatusResponseHelper.Success(EXTERNAL_PROVIDER_LINKED_SUCCESS_MESSAGE);
        }

        if (existingMapping is null)
        {
            // Create a new provider link when no active mapping exists anywhere else.
            await _externalLoginRepository.AddAsync(
                new ExternalLogin
                {
                    UserId = user.Id,
                    LoginProvider = provider.Name,
                    ProviderKey = profile.ProviderUserId
                },
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            InfrastructureLogConstants.ExternalProviderLogs.PROVIDER_LINKED,
            provider.Name,
            currentUserPublicId);

        return OperationStatusResponseHelper.Success(EXTERNAL_PROVIDER_LINKED_SUCCESS_MESSAGE);
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
                       ApplicationConstants.UNAUTHORIZED_REQUEST_MESSAGE,
                       UNAUTHORIZED,
                       StatusCodes.Status401Unauthorized);

        // Resolve provider configuration before locating the link to remove.
        var provider = GetProvider(request.Provider);
        if (provider is null)
        {
            throw new ApiException(INVALID_EXTERNAL_PROVIDER_MESSAGE, AUTH_EXTERNAL_PROVIDER_INVALID);
        }

        // Only the current user's active provider link can be unlinked.
        var externalLogin = await _externalLoginRepository.GetByUserAndProviderAsync(
            user.Id,
            provider.Name,
            cancellationToken);
        if (externalLogin is null)
        {
            throw new ApiException(EXTERNAL_PROVIDER_NOT_LINKED_MESSAGE, AUTH_EXTERNAL_PROVIDER_NOT_LINKED);
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash) && user.ExternalLogins.Count <= 1)
        {
            // Keep at least one usable sign-in method so unlink cannot lock the account out.
            throw new ApiException(LAST_SIGN_IN_METHOD_REQUIRED_MESSAGE, AUTH_LAST_SIGN_IN_METHOD_REQUIRED);
        }

        // Soft-delete the provider mapping so it can be revived by a later link operation.
        externalLogin.IsDeleted = true;
        await _externalLoginRepository.UpdateAsync(externalLogin);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            InfrastructureLogConstants.ExternalProviderLogs.PROVIDER_UNLINKED,
            provider.Name,
            currentUserPublicId);

        return OperationStatusResponseHelper.Success(EXTERNAL_PROVIDER_UNLINKED_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Resolves and normalizes an external provider descriptor from configured options.
    /// </summary>
    /// <param name="provider">The provider name supplied by the caller.</param>
    /// <returns>The normalized provider descriptor, or <c>null</c> when unsupported.</returns>
    private ExternalProviderDescriptorDto GetProvider(string provider)
    {
        // Read raw options first so default party-type configuration can be normalized for persistence.
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
    /// Validates an external identity token against its configured OIDC metadata.
    /// </summary>
    /// <param name="provider">The configured provider name.</param>
    /// <param name="externalToken">The external provider token supplied by the client.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The normalized external identity profile, or <c>null</c> when validation fails.</returns>
    private async Task<ExternalIdentityProfileResponseDto> ValidateAsync(
        string provider,
        string externalToken,
        CancellationToken cancellationToken = default)
    {
        // Missing provider options or token means this external login attempt is not valid.
        var options = GetProviderOptions(provider);
        if (options is null || string.IsNullOrWhiteSpace(externalToken))
        {
            return null;
        }

        try
        {
            // Load OIDC metadata dynamically so provider signing keys stay current.
            var metadataAddress = string.IsNullOrWhiteSpace(options.MetadataAddress)
                ? $"{options.Authority.TrimEnd('/')}/.well-known/openid-configuration"
                : options.MetadataAddress;
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
                ValidIssuers = options.ValidIssuers.Count > 0 ? options.ValidIssuers : [configuration.Issuer],
                ValidateAudience = options.Audiences.Count > 0,
                ValidAudiences = options.Audiences,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            // Validate the external token before reading identity claims from it.
            var principal = _jwtSecurityTokenHandler.ValidateToken(externalToken, validationParameters, out _);
            var email = principal.FindFirstValue(ClaimTypes.Email)
                        ?? principal.FindFirstValue(TokenClaimTypes.EMAIL);
            var fullName = principal.FindFirstValue(ClaimTypes.Name)
                           ?? principal.FindFirstValue(TokenClaimTypes.NAME)
                           ?? principal.FindFirstValue(TokenClaimTypes.GIVEN_NAME);
            var providerUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? principal.FindFirstValue(TokenClaimTypes.SUBJECT);
            var emailVerified = bool.TryParse(
                principal.FindFirstValue(TokenClaimTypes.EMAIL_VERIFIED),
                out var verified)
                && verified;

            if (string.IsNullOrWhiteSpace(providerUserId))
            {
                return null;
            }

            // Providers configured to require verified email cannot link unverified provider emails.
            if (options.RequireVerifiedEmail && !string.IsNullOrWhiteSpace(email) && !emailVerified)
            {
                return null;
            }

            // Return only normalized identity data needed by Haven account provisioning and linking.
            return new ExternalIdentityProfileResponseDto
            {
                ProviderUserId = providerUserId,
                Email = email,
                FullName = fullName,
                EmailVerified = emailVerified || !options.RequireVerifiedEmail
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                InfrastructureLogConstants.ExternalProviderLogs.TOKEN_VALIDATION_FAILED,
                options.Name);
            return null;
        }
    }

    /// <summary>
    /// Creates a new Haven account from a validated external identity profile.
    /// </summary>
    /// <param name="provider">The resolved external provider descriptor.</param>
    /// <param name="profile">The validated external identity profile.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The created user reloaded with authentication graph data.</returns>
    private async Task<User> CreateExternalUserAsync(
        ExternalProviderDescriptorDto provider,
        ExternalIdentityProfileResponseDto profile,
        CancellationToken cancellationToken)
    {
        // Resolve account creation statuses and default party type in one master-data batch.
        var masterDataValues = await _repositories.MasterDataValueRepository.GetByTypeAndValuesAsync(
            [
                new MasterDataValueKeyModel(PARTY_STATUS_TYPE, ACTIVE_STATUS),
                new MasterDataValueKeyModel(USER_STATUS_TYPE, ACTIVE_STATUS),
                new MasterDataValueKeyModel(PARTY_TYPE_TYPE, provider.DefaultPartyType)
            ],
            cancellationToken);
        var partyStatus = MasterDataValueHelper.GetRequired(
            masterDataValues,
            new MasterDataValueRequirementModel(
                PARTY_STATUS_TYPE,
                ACTIVE_STATUS,
                ACTIVE_STATUS_NOT_FOUND_MESSAGE));
        var userStatus = MasterDataValueHelper.GetRequired(
            masterDataValues,
            new MasterDataValueRequirementModel(
                USER_STATUS_TYPE,
                ACTIVE_STATUS,
                ACTIVE_STATUS_NOT_FOUND_MESSAGE));
        var partyType = MasterDataValueHelper.GetRequired(
            masterDataValues,
            new MasterDataValueRequirementModel(
                PARTY_TYPE_TYPE,
                provider.DefaultPartyType,
                DEFAULT_PARTY_TYPE_NOT_FOUND_MESSAGE));
        var fullName = string.IsNullOrWhiteSpace(profile.FullName)
            ? profile.Email ?? profile.ProviderUserId
            : profile.FullName.NormalizeOptional();
        var normalizedEmail = profile.Email.NormalizeEmail();
        var provision = new ExternalAccountProvisionRequestDto
        {
            PartyTypeId = partyType.Id,
            PartyStatusId = partyStatus.Id,
            UserStatusId = userStatus.Id,
            UserName = await GenerateUniqueUserNameAsync(normalizedEmail, fullName, cancellationToken),
            Email = normalizedEmail,
            FullName = fullName,
            EmailConfirmed = string.IsNullOrWhiteSpace(normalizedEmail)
                             || !provider.RequireVerifiedEmail
                             || profile.EmailVerified
        };

        // External account provisioning must create all identity rows atomically.
        return await _unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                // External provisioning creates the identity graph atomically:
                // Party + User + UserParty + ExternalLogin.
                var party = provision.Adapt<Party>();
                var user = provision.Adapt<User>();
                user.CurrentParty = party;
                user.LastLoginAt = DateTime.UtcNow;

                await _repositories.PartyRepository.AddAsync(party, ct);
                await _repositories.UserRepository.AddAsync(user, ct);
                await _repositories.UserPartyRepository.AddAsync(new UserParty { User = user, Party = party }, ct);
                await _externalLoginRepository.AddAsync(
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
                           EXTERNAL_USER_NOT_LOADED_MESSAGE,
                           StatusCodes.Status500InternalServerError);
            },
            cancellationToken);
    }

    /// <summary>
    /// Generates a unique local username for an externally provisioned account.
    /// </summary>
    /// <param name="email">The normalized external email address, when available.</param>
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
        // Provider matching is case-insensitive because clients may send display-style provider names.
        return _externalAuthenticationOptions.Providers.FirstOrDefault(
            x => string.Equals(x.Name, provider, StringComparison.OrdinalIgnoreCase));
    }

}
