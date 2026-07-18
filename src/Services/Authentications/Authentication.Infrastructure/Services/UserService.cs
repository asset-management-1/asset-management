namespace Authentication.Infrastructure.Services;

/// <summary>
/// Provides infrastructure operations for current-user profile and context flows.
/// </summary>
public class UserService : IUserService
{
    private readonly AuthenticationRepositoryDependencies _repositories;
    private readonly AuthenticationKycRepositoryDependencies _kycRepositories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IObjectStorageService _objectStorageService;
    private readonly IImageOptimizationService _imageOptimizationService;
    private readonly ExternalAuthenticationOptions _externalAuthenticationOptions;
    private readonly R2StorageOptions _r2StorageOptions;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// Creates the current-user service with profile, context, KYC, and storage dependencies.
    /// </summary>
    /// <param name="repositories">The grouped authentication repositories used by profile and context operations.</param>
    /// <param name="kycRepositories">The grouped repositories used by KYC and document persistence.</param>
    /// <param name="unitOfWork">The unit of work used for transactional writes.</param>
    /// <param name="authService">The current authenticated-principal accessor.</param>
    /// <param name="supportDependencies">The grouped storage and option dependencies for user flows.</param>
    /// <param name="logger">The user-service logger.</param>
    public UserService(
        AuthenticationRepositoryDependencies repositories,
        AuthenticationKycRepositoryDependencies kycRepositories,
        IUnitOfWork unitOfWork,
        IAuthService authService,
        UserServiceSupportDependencies supportDependencies,
        ILogger<UserService> logger)
    {
        _repositories = repositories;
        _kycRepositories = kycRepositories;
        _unitOfWork = unitOfWork;
        _authService = authService;
        _objectStorageService = supportDependencies.ObjectStorageService;
        _imageOptimizationService = supportDependencies.ImageOptimizationService;
        _externalAuthenticationOptions = supportDependencies.ExternalAuthenticationOptions;
        _r2StorageOptions = supportDependencies.R2StorageOptions;
        _logger = logger;
    }

    /// <summary>
    /// Loads current-user profile information.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The current user's profile, context, and external-link state.</returns>
    public async Task<UserInfoResponseDto> GetUserInfoAsync(CancellationToken cancellationToken = default)
    {
        // Step 1: Resolve both identity and session claims because Party context belongs to the client session.
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);
        var currentSessionPublicId = _authService.SessionId()
                                     ?? throw new HttpStatusCodeException(
                                         ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                         UNAUTHORIZED,
                                         StatusCodes.Status401Unauthorized);

        // Step 2: Load the session-aware projection without caching it globally by User.
        var response = await _repositories.UserRepository.GetUserInfoResponseByPublicIdAsync(
            currentUserPublicId,
            currentSessionPublicId,
            cancellationToken);
        if (response is null)
        {
            throw new HttpStatusCodeException(
                ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                UNAUTHORIZED,
                StatusCodes.Status401Unauthorized);
        }

        // Merge configured providers with linked-provider state so UI can render every provider option.
        response.ExternalProviders = BuildExternalProviderResponses(response.ExternalProviders);

        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.USER_INFO_LOADED,
            currentUserPublicId);

        return response;
    }

    /// <summary>
    /// Updates profile fields for the current authenticated user.
    /// </summary>
    /// <param name="request">The profile update payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The profile update result.</returns>
    public async Task<OperationStatusResponseDto> UpdateUserInfoAsync(
        UpdateUserInfoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Load the tracked user so profile fields can be applied in the current unit of work.
        var user = await LoadTrackedCurrentUserAsync(cancellationToken);
        var previousAvatarUrl = user.AvatarUrl;
        var phoneNumber = request.PhoneNumber;

        // Phone number uniqueness is checked only when the submitted value changes.
        if (!string.IsNullOrWhiteSpace(phoneNumber)
            && !string.Equals(user.PhoneNumber, phoneNumber, StringComparison.OrdinalIgnoreCase)
            && await _repositories.UserRepository.PhoneNumberExistsAsync(phoneNumber, cancellationToken))
        {
            throw new ApiException(
                ApplicationErrorConstants.AccountErrors.PHONE_NUMBER_ALREADY_EXISTS_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_USER_ALREADY_EXISTS);
        }

        // Resolve optional lookup data and all linked parties before applying profile snapshots.
        var gender = await ResolveOptionalGenderAsync(request.Gender, cancellationToken);
        var parties = await LoadTrackedUserPartiesAsync(user, cancellationToken);
        var uploadedAvatar = request.AvatarFile is null
            ? null
            : await UploadAvatarAsync(user.PublicId, request.AvatarFile, cancellationToken);
        var avatarUrl = ObjectStorageHelper.BuildObjectUrl(
            _r2StorageOptions.PublicBaseUrl,
            uploadedAvatar?.ObjectKey);

        // Profile fields live on the identity user while display/contact snapshots are kept in linked parties.
        ApplyProfileUpdate(user, request, gender, phoneNumber, avatarUrl);
        SyncPartyProfile(parties, request.FullName, phoneNumber, null);

        try
        {
            // Persist only after optional avatar upload succeeds so the stored URL points to a durable object.
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                InfrastructureLogConstants.UserLogs.USER_INFO_UPDATE_PERSISTENCE_FAILED,
                user.PublicId);

            await CleanupUploadedObjectsAsync(
                user.PublicId,
                [uploadedAvatar],
                cancellationToken);

            throw new ApiException(
                ApplicationErrorConstants.ProfileErrors.USER_INFO_UPDATE_FAILED_MESSAGE,
                ApplicationErrorConstants.ProfileErrorCodes.AUTH_PROFILE_UPDATE_FAILED,
                StatusCodes.Status500InternalServerError);
        }

        if (uploadedAvatar is not null)
        {
            // Delete the superseded avatar only after the new URL is committed.
            // Cleanup cannot invalidate the profile update.
            var cleanupResult = await ObjectStorageHelper.CleanupObjectKeysAsync(
                _objectStorageService,
                [ObjectStorageHelper.GetObjectKey(_r2StorageOptions.PublicBaseUrl, previousAvatarUrl)],
                cancellationToken);
            if (cleanupResult.HasFailures)
            {
                _logger.LogWarning(
                    InfrastructureLogConstants.UserLogs.UPLOADED_OBJECT_CLEANUP_FAILED,
                    user.PublicId);
            }
        }

        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.USER_INFO_UPDATED,
            user.PublicId);

        return OperationStatusResponseHelper.Success(
            ApplicationMessageConstants.ProfileMessages.USER_INFO_UPDATED_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Validates a change-email request and returns notification data.
    /// </summary>
    /// <param name="request">The change-email request payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The validated change-email data.</returns>
    public async Task<ChangeEmailStartResponseDto> PrepareChangeEmailAsync(
        ChangeEmailRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Load the current user because change-email validation depends on the existing email.
        var user = await LoadTrackedCurrentUserAsync(cancellationToken);
        var normalizedNewEmail = request.NewEmail;

        if (string.Equals(user.Email, normalizedNewEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(
                ApplicationErrorConstants.AccountErrors.EMAIL_ALREADY_EXISTS_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_USER_ALREADY_EXISTS);
        }

        // Confirm that the new email is not owned by another active user.
        await EnsureEmailAvailableAsync(normalizedNewEmail, user.Id, cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.CHANGE_EMAIL_PREPARED,
            user.PublicId);

        return new ChangeEmailStartResponseDto
        {
            OldEmail = user.Email,
            NewEmail = normalizedNewEmail
        };
    }

    /// <summary>
    /// Applies a verified change-email request to the current user.
    /// </summary>
    /// <param name="request">The verified change-email payload.</param>
    /// <param name="currentUserPublicId">The current authenticated user's public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The email-change result.</returns>
    public async Task<OperationStatusResponseDto> VerifyChangeEmailAsync(
        VerifyChangeEmailOtpRequestDto request,
        Guid currentUserPublicId,
        CancellationToken cancellationToken = default)
    {
        var normalizedNewEmail = request.NewEmail;

        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(
                async ct =>
                {
                    // Step 1: Lock the User so concurrent verified-email mutations serialize on one account.
                    var user = await _repositories.UserRepository.GetTrackedByPublicIdForUpdateAsync(
                                   currentUserPublicId,
                                   ct)
                               ?? throw new HttpStatusCodeException(
                                   ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                   UNAUTHORIZED,
                                   StatusCodes.Status401Unauthorized);
                    var oldEmail = user.Email;

                    // Step 2: Recheck uniqueness after the lock because the OTP window permits competing requests.
                    await EnsureEmailAvailableAsync(normalizedNewEmail, user.Id, ct);

                    // Step 3: Apply the verified email and preserve legacy email-based usernames when present.
                    user.Email = normalizedNewEmail;
                    user.EmailConfirmed = true;
                    if (string.Equals(user.UserName, oldEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        user.UserName = normalizedNewEmail;
                    }

                    // Step 4: Synchronize Party contact snapshots in the same transaction as the identity row.
                    SyncPartyProfile(await LoadTrackedUserPartiesAsync(user, ct), null, null, normalizedNewEmail);
                },
                cancellationToken);
        }
        catch (DbUpdateException exception)
            when (PostgreSqlExceptionHelper.GetUniqueConstraintName(exception) == USER_EMAIL_UNIQUE_CONSTRAINT)
        {
            throw new ApiException(
                ApplicationErrorConstants.AccountErrors.EMAIL_ALREADY_EXISTS_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_USER_ALREADY_EXISTS);
        }

        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.CHANGE_EMAIL_VERIFIED,
            currentUserPublicId);

        return OperationStatusResponseHelper.Success(
            ApplicationMessageConstants.ProfileMessages.CHANGE_EMAIL_COMPLETED_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Submits identity-document KYC data and private document metadata for manual admin review.
    /// </summary>
    /// <param name="request">The KYC submission payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The KYC submission result.</returns>
    public async Task<KycSubmissionResponseDto> SubmitKycAsync(
        SubmitKycRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Load the current User and all linked Parties for the account-wide KYC guard.
        var user = await LoadTrackedCurrentUserAsync(cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.KYC_SUBMISSION_STARTED,
            user.PublicId);

        var parties = (await LoadTrackedUserPartiesAsync(user, cancellationToken)).ToList();

        if (parties.Count == 0)
        {
            throw new ApiException(
                ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID,
                StatusCodes.Status500InternalServerError);
        }

        // Step 2: Resolve the Party selected by this client session; KYC persistence must not choose an arbitrary Party.
        var currentSessionPublicId = _authService.SessionId()
                                     ?? throw new HttpStatusCodeException(
                                         ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                         UNAUTHORIZED,
                                         StatusCodes.Status401Unauthorized);
        var currentSession = await _repositories.RefreshTokenRepository.GetByUserAndSessionPublicIdAsync(
                                 user.Id,
                                 currentSessionPublicId,
                                 cancellationToken)
                             ?? throw new HttpStatusCodeException(
                                 ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                 UNAUTHORIZED,
                                 StatusCodes.Status401Unauthorized);
        var currentParty = parties.FirstOrDefault(x => x.Id == currentSession.CurrentPartyId)
                           ?? throw new ApiException(
                               ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                               ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID);
        var persistenceParties = new List<Party> { currentParty };

        // Step 3: Resolve KYC lookup values before applying the account-wide duplicate guard.
        var kycMasterData = await ResolveKycMasterDataAsync(request, cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.KYC_MASTER_DATA_RESOLVED,
            user.PublicId);

        await EnsureKycCanBeSubmittedAsync(
            new KycSubmissionGuardModel
            {
                CurrentUserPublicId = user.PublicId,
                Parties = parties,
                IdentifierTypeIds = kycMasterData.SupportedIdentifierTypes.Select(x => x.Id).ToList(),
                RejectedStatusId = kycMasterData.RejectedKycStatus.Id
            },
            cancellationToken);

        // Identity information is staged for admin review; profile fields are not updated until approval.
        await UpsertPartyIdentifierAsync(
            persistenceParties,
            request,
            kycMasterData,
            cancellationToken);
        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.KYC_IDENTIFIER_STAGED,
            user.PublicId);

        // Private scans are uploaded before metadata persistence; no object key is returned to the API caller.
        var uploadedDocuments = await UploadKycDocumentsAsync(user.PublicId, request, cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.KYC_UPLOAD_COMPLETED,
            user.PublicId);

        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(
                async ct =>
                {
                    // Persist document metadata and link each scan only to the Party selected by this session.
                    var frontDocument = BuildDocument(
                        new KycDocumentBuildModel
                        {
                            File = request.FrontFile,
                            Upload = uploadedDocuments.Front,
                            MasterData = kycMasterData
                        });
                    var backDocument = uploadedDocuments.Back is null
                        ? null
                        : BuildDocument(
                            new KycDocumentBuildModel
                            {
                                File = request.BackFile,
                                Upload = uploadedDocuments.Back,
                                MasterData = kycMasterData
                            });

                    await _kycRepositories.DocumentRepository.AddAsync(frontDocument, ct);
                    if (backDocument is not null)
                    {
                        await _kycRepositories.DocumentRepository.AddAsync(backDocument, ct);
                    }

                    await CreateDocumentLinksAsync(
                        new KycDocumentLinkCreationModel
                        {
                            Parties = persistenceParties,
                            FrontDocument = frontDocument,
                            BackDocument = backDocument,
                            MasterData = kycMasterData
                        },
                        ct);

                },
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                InfrastructureLogConstants.UserLogs.KYC_SUBMISSION_PERSISTENCE_FAILED,
                user.PublicId);

            await CleanupUploadedObjectsAsync(
                user.PublicId,
                uploadedDocuments.UploadedObjects,
                cancellationToken);

            throw new ApiException(
                ApplicationErrorConstants.KycErrors.KYC_SUBMISSION_FAILED_MESSAGE,
                ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID,
                StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.KYC_SUBMITTED,
            user.PublicId);

        return new KycSubmissionResponseDto
        {
            IsSubmitted = true,
            Status = KycStatusEnum.Pending,
            Message = ApplicationMessageConstants.KycMessages.KYC_SUBMITTED_SUCCESS_MESSAGE
        };
    }

    /// <summary>
    /// Switches the current user's active party context.
    /// </summary>
    /// <param name="request">The target party-context switch payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resolved current context and available contexts after switching.</returns>
    public async Task<SwitchPartyResponseDto> SwitchPartyAsync(
        SwitchPartyRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Resolve authenticated identifiers before entering the protected context-switch transaction.
        var targetContext = request.TargetContext;
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);
        var currentSessionPublicId = _authService.SessionId()
                                     ?? throw new HttpStatusCodeException(
                                         ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                         UNAUTHORIZED,
                                         StatusCodes.Status401Unauthorized);
        var targetPartyType = EnumConvertHelper<PartyTypeEnum>
            .TryConvertStringToEnum(targetContext)?
            .ToMasterDataCode();
        User switchedUser = null;

        // Step 2: Lock the User, recheck matching relations, and update only the current refresh-token session.
        await _unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                switchedUser = await _repositories.UserRepository.GetTrackedByPublicIdForUpdateAsync(
                                   currentUserPublicId,
                                   ct)
                               ?? throw new HttpStatusCodeException(
                                   ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                   UNAUTHORIZED,
                                   StatusCodes.Status401Unauthorized);
                var session = await _repositories.RefreshTokenRepository.GetByUserAndSessionPublicIdAsync(
                                  switchedUser.Id,
                                  currentSessionPublicId,
                                  ct)
                              ?? throw new HttpStatusCodeException(
                                  ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                  UNAUTHORIZED,
                                  StatusCodes.Status401Unauthorized);
                var matchingRelations = await _repositories.UserPartyRepository.GetAllByUserIdAndPartyTypeAsync(
                    switchedUser.Id,
                    targetPartyType,
                    ct);

                // Preserve the session Party when it already has the requested type; otherwise use the earliest match.
                var selectedPartyId = matchingRelations
                                          .FirstOrDefault(relation => relation.PartyId == session.CurrentPartyId)?
                                          .PartyId
                                      ?? matchingRelations.FirstOrDefault()?.PartyId
                                      ?? await CreatePartyContextAsync(switchedUser, targetPartyType, ct);

                session.CurrentPartyId = selectedPartyId;
                await _repositories.RefreshTokenRepository.UpdateAsync(session);
            },
            cancellationToken);

        // Step 3: Rebuild available contexts after commit so the response includes a newly created Party type.
        var availableContexts = (await _repositories.UserPartyRepository.GetActiveByUserIdAsync(
                switchedUser.Id,
                cancellationToken))
            .Select(x => EnumConvertHelper<PartyTypeEnum>
                .TryConvertStringToEnum(x.Party?.PartyType?.Code)?
                .ToString()
                .ToLowerInvariant())
            .Where(x => x is not null)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (!availableContexts.Contains(targetContext, StringComparer.OrdinalIgnoreCase))
        {
            availableContexts.Add(targetContext);
        }

        _logger.LogInformation(
            InfrastructureLogConstants.ContextLogs.CONTEXT_SWITCHED,
            targetContext,
            switchedUser.PublicId);

        return new SwitchPartyResponseDto
        {
            IsSuccess = true,
            Message = ApplicationMessageConstants.AccountMessages.SWITCH_PARTY_SUCCESS_MESSAGE,
            CurrentContext = EnumConvertHelper<PartyTypeEnum>.TryConvertStringToEnum(targetContext)
                             ?? throw new InvalidOperationException(
                                 string.Format(
                                     InfrastructureErrorConstants.PartyContextErrors.UNSUPPORTED_PARTY_CONTEXT_VALUE_MESSAGE,
                                     targetContext)),
            AvailableContexts = availableContexts
                .Select(EnumConvertHelper<PartyTypeEnum>.TryConvertStringToEnum)
                .Where(context => context.HasValue)
                .Select(context => context.Value)
                .ToList()
        };
    }

    /// <summary>
    /// Loads the current authenticated user as a tracked entity.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked current user.</returns>
    private async Task<User> LoadTrackedCurrentUserAsync(CancellationToken cancellationToken)
    {
        // The auth handler has already validated the token; here we resolve its public user id claim.
        var currentUserPublicId = _authService.UserId();

        if (!currentUserPublicId.HasValue)
        {
            _logger.LogWarning(InfrastructureLogConstants.UserLogs.CURRENT_USER_CLAIM_MISSING);

            throw new HttpStatusCodeException(
                ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                UNAUTHORIZED,
                StatusCodes.Status401Unauthorized);
        }

        // Load a tracked user because callers mutate identity/profile/context fields.
        var user = await _repositories.UserRepository.GetTrackedByPublicIdAsync(
            currentUserPublicId.Value,
            cancellationToken);
        if (user is not null)
        {
            return user;
        }

        _logger.LogWarning(
            InfrastructureLogConstants.UserLogs.CURRENT_USER_NOT_FOUND,
            currentUserPublicId.Value);

        throw new HttpStatusCodeException(
            ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
            UNAUTHORIZED,
            StatusCodes.Status401Unauthorized);
    }

    /// <summary>
    /// Loads active linked parties as tracked entities for sync writes.
    /// </summary>
    /// <param name="user">The current tracked user entity.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The active linked parties for the user.</returns>
    private async Task<IReadOnlyList<Party>> LoadTrackedUserPartiesAsync(
        User user,
        CancellationToken cancellationToken)
    {
        // Start from user-party links because a user can own both tenant and landlord contexts.
        var partyIds = (await _repositories.UserPartyRepository.GetActiveByUserIdAsync(user.Id, cancellationToken))
            .Select(x => x.PartyId)
            .ToList();
        // Return tracked parties because callers synchronize profile/contact snapshots.
        return await _repositories.PartyRepository.GetTrackedByIdsAsync(partyIds, cancellationToken);
    }

    /// <summary>
    /// Applies optional profile fields to the tracked user entity.
    /// </summary>
    /// <param name="user">The tracked current user entity.</param>
    /// <param name="request">The profile update payload.</param>
    /// <param name="gender">The resolved gender master data, if supplied.</param>
    /// <param name="phoneNumber">The submitted phone number, if supplied.</param>
    /// <param name="avatarUrl">The backend-uploaded avatar URL, if supplied.</param>
    private static void ApplyProfileUpdate(
        User user,
        UpdateUserInfoRequestDto request,
        MasterDataValue gender,
        string phoneNumber,
        string avatarUrl)
    {
        // Mapster applies simple partial profile fields and leaves omitted values untouched.
        request.Adapt(user);

        // Phone, avatar, and gender stay explicit because they use separate validation, upload, or lookup flows.
        if (phoneNumber is not null)
        {
            user.PhoneNumber = phoneNumber;
        }

        if (avatarUrl is not null)
        {
            user.AvatarUrl = avatarUrl;
        }

        if (gender is not null)
        {
            user.GenderId = gender.Id;
        }
    }

    /// <summary>
    /// Synchronizes account profile/contact snapshots to all linked parties.
    /// </summary>
    /// <param name="parties">The tracked linked parties to update.</param>
    /// <param name="fullName">The optional full name to sync as display name.</param>
    /// <param name="phoneNumber">The optional phone number to sync as primary phone.</param>
    /// <param name="email">The optional email to sync as primary email.</param>
    private static void SyncPartyProfile(
        IEnumerable<Party> parties,
        string fullName,
        string phoneNumber,
        string email)
    {
        // Keep party snapshots aligned with account-level profile fields across every linked context.
        foreach (var party in parties)
        {
            if (fullName is not null)
            {
                party.DisplayName = fullName;
            }

            if (phoneNumber is not null)
            {
                party.PrimaryPhone = phoneNumber;
            }

            if (email is not null)
            {
                party.PrimaryEmail = email;
            }
        }
    }

    /// <summary>
    /// Ensures that a normalized email is not owned by another non-deleted user.
    /// </summary>
    /// <param name="normalizedEmail">The normalized email address.</param>
    /// <param name="currentUserId">The current internal user identifier to exclude.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>A task that completes when the email is available.</returns>
    private async Task EnsureEmailAvailableAsync(
        string normalizedEmail,
        long currentUserId,
        CancellationToken cancellationToken)
    {
        // Look up by normalized email and ignore the current user's own row.
        var existingUser = await _repositories.UserRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (existingUser is not null && existingUser.Id != currentUserId)
        {
            throw new ApiException(
                ApplicationErrorConstants.AccountErrors.EMAIL_ALREADY_EXISTS_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_USER_ALREADY_EXISTS);
        }
    }

    /// <summary>
    /// Resolves optional profile gender into a master-data value.
    /// </summary>
    /// <param name="gender">The optional gender code from the request.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The resolved gender master-data value, or <c>null</c> when not supplied.</returns>
    private async Task<MasterDataValue> ResolveOptionalGenderAsync(
        string gender,
        CancellationToken cancellationToken)
    {
        // Gender is optional for profile updates; resolve it only when supplied.
        if (string.IsNullOrWhiteSpace(gender))
        {
            return null;
        }

        // Supplied gender must be an active master-data value.
        return await _repositories.MasterDataValueRepository.GetByTypeAndValueAsync(
                   PROFILE_GENDER_TYPE,
                   gender,
                   cancellationToken)
               ?? throw new ApiException(
                   ApplicationErrorConstants.ProfileErrors.INVALID_GENDER_MESSAGE,
                   ApplicationErrorConstants.ProfileErrorCodes.AUTH_INVALID_GENDER);
    }

    /// <summary>
    /// Resolves all master data needed to persist a KYC submission.
    /// </summary>
    /// <param name="request">The KYC submission payload.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The resolved master-data context for KYC persistence.</returns>
    private async Task<KycMasterDataContextModel> ResolveKycMasterDataAsync(
        SubmitKycRequestDto request,
        CancellationToken cancellationToken)
    {
        // CCCD stores two side-specific links; passport stores one primary scan link.
        var cccdIdentifierType = IdentifierTypeEnum.Cccd.ToMasterDataCode();
        var passportIdentifierType = IdentifierTypeEnum.Passport.ToMasterDataCode();
        var pendingKycStatus = KycStatusEnum.Pending.ToMasterDataCode();
        var rejectedKycStatus = KycStatusEnum.Rejected.ToMasterDataCode();
        var requiresBackFile = request.IdentifierType == cccdIdentifierType;
        var frontLinkType = requiresBackFile ? NATIONAL_ID_FRONT_SCAN_LINK_TYPE : NATIONAL_ID_SCAN_LINK_TYPE;
        var backLinkType = requiresBackFile ? NATIONAL_ID_BACK_SCAN_LINK_TYPE : null;

        // Resolve every KYC master-data value in one batch to avoid repeated database round-trips.
        var masterDataKeys = new List<MasterDataValueKeyModel>
        {
            new(IDENTIFIER_TYPE_TYPE, request.IdentifierType),
            new(IDENTIFIER_TYPE_TYPE, cccdIdentifierType),
            new(IDENTIFIER_TYPE_TYPE, passportIdentifierType),
            new(PROFILE_GENDER_TYPE, request.GenderOnDocument),
            new(DOCUMENT_TYPE_TYPE, NATIONAL_ID_DOCUMENT_TYPE),
            new(STORAGE_PROVIDER_TYPE, R2_STORAGE_PROVIDER),
            new(DOCUMENT_STATUS_TYPE, UPLOADED_DOCUMENT_STATUS),
            new(ENTITY_TYPE_TYPE, PARTY_ENTITY_TYPE),
            new(DOCUMENT_LINK_TYPE_TYPE, frontLinkType),
            new(DOCUMENT_LINK_STATUS_TYPE, ACTIVE_DOCUMENT_LINK_STATUS),
            new(KYC_STATUS_TYPE, pendingKycStatus),
            new(KYC_STATUS_TYPE, rejectedKycStatus)
        };
        if (!string.IsNullOrWhiteSpace(backLinkType))
        {
            masterDataKeys.Add(new MasterDataValueKeyModel(DOCUMENT_LINK_TYPE_TYPE, backLinkType));
        }

        var masterDataValues = await _repositories.MasterDataValueRepository.GetByTypeAndValuesAsync(
            masterDataKeys,
            cancellationToken);

        // KYC needs several master values; resolve them from one batched DB lookup to avoid repeated round-trips.
        return new KycMasterDataContextModel
        {
            IdentifierType = MasterDataValueHelper.GetRequired(
                masterDataValues,
                new MasterDataValueRequirementModel(
                    IDENTIFIER_TYPE_TYPE,
                    request.IdentifierType,
                    ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID)),
            SupportedIdentifierTypes =
            [
                MasterDataValueHelper.GetRequired(
                    masterDataValues,
                    new MasterDataValueRequirementModel(
                        IDENTIFIER_TYPE_TYPE,
                        cccdIdentifierType,
                        ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                        ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID)),
                MasterDataValueHelper.GetRequired(
                    masterDataValues,
                    new MasterDataValueRequirementModel(
                        IDENTIFIER_TYPE_TYPE,
                        passportIdentifierType,
                        ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                        ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID))
            ],
            Gender = MasterDataValueHelper.GetRequired(
                masterDataValues,
                new MasterDataValueRequirementModel(
                    PROFILE_GENDER_TYPE,
                    request.GenderOnDocument,
                    ApplicationErrorConstants.ProfileErrors.INVALID_GENDER_MESSAGE,
                    ApplicationErrorConstants.ProfileErrorCodes.AUTH_INVALID_GENDER)),
            DocumentType = MasterDataValueHelper.GetRequired(
                masterDataValues,
                new MasterDataValueRequirementModel(
                    DOCUMENT_TYPE_TYPE,
                    NATIONAL_ID_DOCUMENT_TYPE,
                    ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID)),
            StorageProvider = MasterDataValueHelper.GetRequired(
                masterDataValues,
                new MasterDataValueRequirementModel(
                    STORAGE_PROVIDER_TYPE,
                    R2_STORAGE_PROVIDER,
                    ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID)),
            DocumentStatus = MasterDataValueHelper.GetRequired(
                masterDataValues,
                new MasterDataValueRequirementModel(
                    DOCUMENT_STATUS_TYPE,
                    UPLOADED_DOCUMENT_STATUS,
                    ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID)),
            EntityType = MasterDataValueHelper.GetRequired(
                masterDataValues,
                new MasterDataValueRequirementModel(
                    ENTITY_TYPE_TYPE,
                    PARTY_ENTITY_TYPE,
                    ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID)),
            FrontLinkType = MasterDataValueHelper.GetRequired(
                masterDataValues,
                new MasterDataValueRequirementModel(
                    DOCUMENT_LINK_TYPE_TYPE,
                    frontLinkType,
                    ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID)),
            BackLinkType = string.IsNullOrWhiteSpace(backLinkType)
                ? null
                : MasterDataValueHelper.GetRequired(
                    masterDataValues,
                    new MasterDataValueRequirementModel(
                        DOCUMENT_LINK_TYPE_TYPE,
                        backLinkType,
                        ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                        ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID)),
            LinkStatus = MasterDataValueHelper.GetRequired(
                masterDataValues,
                new MasterDataValueRequirementModel(
                    DOCUMENT_LINK_STATUS_TYPE,
                    ACTIVE_DOCUMENT_LINK_STATUS,
                    ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID)),
            KycStatus = MasterDataValueHelper.GetRequired(
                masterDataValues,
                new MasterDataValueRequirementModel(
                    KYC_STATUS_TYPE,
                    pendingKycStatus,
                    ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID)),
            RejectedKycStatus = MasterDataValueHelper.GetRequired(
                masterDataValues,
                new MasterDataValueRequirementModel(
                    KYC_STATUS_TYPE,
                    rejectedKycStatus,
                    ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID))
        };
    }

    /// <summary>
    /// Uploads KYC document scans to private object storage.
    /// </summary>
    /// <param name="currentUserPublicId">The current user's public identifier.</param>
    /// <param name="request">The KYC submission payload.</param>
    /// <param name="cancellationToken">The token used to cancel uploads.</param>
    /// <returns>The private upload metadata for submitted document slots.</returns>
    private async Task<KycUploadedDocumentsModel> UploadKycDocumentsAsync(
        Guid currentUserPublicId,
        SubmitKycRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Build the file list from the selected document type so Passport stays a one-file upload.
            var files = new List<ObjectStorageUploadFileModel>
            {
                new()
                {
                    SlotName = FRONT_OBJECT_SLOT,
                    ObjectTag = FRONT_OBJECT_SLOT,
                    File = request.FrontFile,
                    IsRequired = true
                }
            };
            if (request.IdentifierType == IdentifierTypeEnum.Cccd.ToMasterDataCode())
            {
                files.Add(new ObjectStorageUploadFileModel
                {
                    SlotName = BACK_OBJECT_SLOT,
                    ObjectTag = BACK_OBJECT_SLOT,
                    File = request.BackFile,
                    IsRequired = true
                });
            }

            // Decode all submitted scans before preserving their original bytes under the private KYC prefix.
            var uploadResult = await ObjectStorageHelper.UploadOwnerScopedValidatedImageFormFilesAsync(
                _objectStorageService,
                _imageOptimizationService,
                new ObjectStorageUploadBatchRequestModel
                {
                    ConfiguredPrefix = _r2StorageOptions.KycObjectPrefix,
                    DefaultPrefix = DEFAULT_KYC_OBJECT_PREFIX,
                    OwnerPublicId = currentUserPublicId,
                    Files = files
                },
                cancellationToken);

            return new KycUploadedDocumentsModel
            {
                Front = uploadResult.GetUpload(FRONT_OBJECT_SLOT),
                Back = uploadResult.GetUpload(BACK_OBJECT_SLOT)
            };
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, InfrastructureLogConstants.UserLogs.KYC_UPLOAD_FAILED, currentUserPublicId);
            throw new ApiException(
                IMAGE_FILE_INVALID_MESSAGE,
                ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID,
                StatusCodes.Status400BadRequest);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                InfrastructureLogConstants.UserLogs.KYC_UPLOAD_FAILED,
                currentUserPublicId);

            throw new ApiException(
                ApplicationErrorConstants.KycErrors.KYC_SUBMISSION_FAILED_MESSAGE,
                ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_UPLOAD_FAILED,
                StatusCodes.Status503ServiceUnavailable);
        }
    }

    /// <summary>
    /// Uploads an optional profile avatar image to object storage.
    /// </summary>
    /// <param name="currentUserPublicId">The current user's public identifier.</param>
    /// <param name="avatarFile">The avatar image supplied through multipart form data.</param>
    /// <param name="cancellationToken">The token used to cancel upload.</param>
    /// <returns>The uploaded avatar metadata.</returns>
    private async Task<ObjectUploadResponseModel> UploadAvatarAsync(
        Guid currentUserPublicId,
        IFormFile avatarFile,
        CancellationToken cancellationToken)
    {
        try
        {
            // Decode the avatar before preserving its original bytes with the shared owner-scoped key rules.
            var uploadResult = await ObjectStorageHelper.UploadOwnerScopedValidatedImageFormFilesAsync(
                _objectStorageService,
                _imageOptimizationService,
                new ObjectStorageUploadBatchRequestModel
                {
                    ConfiguredPrefix = _r2StorageOptions.AvatarObjectPrefix,
                    DefaultPrefix = DEFAULT_AVATAR_OBJECT_PREFIX,
                    OwnerPublicId = currentUserPublicId,
                    Files =
                    [
                        new ObjectStorageUploadFileModel
                        {
                            SlotName = AVATAR_OBJECT_SLOT,
                            ObjectTag = AVATAR_OBJECT_SLOT,
                            File = avatarFile
                        }
                    ]
                },
                cancellationToken);
            _logger.LogInformation(
                InfrastructureLogConstants.UserLogs.USER_AVATAR_UPLOAD_COMPLETED,
                currentUserPublicId);

            return uploadResult.GetUpload(AVATAR_OBJECT_SLOT);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                InfrastructureLogConstants.UserLogs.USER_AVATAR_UPLOAD_FAILED,
                currentUserPublicId);

            throw new ApiException(
                IMAGE_FILE_INVALID_MESSAGE,
                ApplicationErrorConstants.ProfileErrorCodes.AUTH_PROFILE_UPLOAD_FAILED,
                StatusCodes.Status400BadRequest);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                InfrastructureLogConstants.UserLogs.USER_AVATAR_UPLOAD_FAILED,
                currentUserPublicId);

            throw new ApiException(
                ApplicationErrorConstants.ProfileErrors.AVATAR_UPLOAD_FAILED_MESSAGE,
                ApplicationErrorConstants.ProfileErrorCodes.AUTH_PROFILE_UPLOAD_FAILED,
                StatusCodes.Status503ServiceUnavailable);
        }
    }

    /// <summary>
    /// Deletes already-uploaded objects and logs cleanup failures for this user flow.
    /// </summary>
    /// <param name="currentUserPublicId">The current user's public identifier used for safe logging.</param>
    /// <param name="uploads">The uploaded object metadata to clean up.</param>
    /// <param name="cancellationToken">The token used to cancel cleanup calls.</param>
    /// <returns>A task that completes after best-effort cleanup.</returns>
    private async Task CleanupUploadedObjectsAsync(
        Guid currentUserPublicId,
        IEnumerable<ObjectUploadResponseModel> uploads,
        CancellationToken cancellationToken)
    {
        // Use the shared cleanup implementation and keep only user-flow logging local to this service.
        var cleanupResult = await ObjectStorageHelper.CleanupUploadedObjectsAsync(
            _objectStorageService,
            uploads,
            cancellationToken);
        if (cleanupResult.HasFailures)
        {
            _logger.LogWarning(
                InfrastructureLogConstants.UserLogs.UPLOADED_OBJECT_CLEANUP_FAILED,
                currentUserPublicId);
        }
    }

    /// <summary>
    /// Builds document metadata for one private KYC upload.
    /// </summary>
    /// <param name="request">The document build request.</param>
    /// <returns>The unsaved document entity.</returns>
    private static Document BuildDocument(KycDocumentBuildModel request)
    {
        // Store private object metadata only; KYC document URLs are intentionally not exposed.
        return new Document
        {
            DocumentTypeId = request.MasterData.DocumentType.Id,
            FileName = Path.GetFileName(request.Upload.ObjectKey),
            OriginalFileName = request.File.FileName,
            ContentType = request.Upload.ContentType,
            // Persist the canonical stored extension while retaining the user-supplied name separately for audit.
            FileExtension = Path.GetExtension(request.Upload.ObjectKey),
            FileSize = request.Upload.FileSize,
            StorageProviderId = request.MasterData.StorageProvider.Id,
            StoragePath = request.Upload.ObjectKey,
            FileUrl = null,
            Checksum = request.Upload.Checksum,
            StatusId = request.MasterData.DocumentStatus.Id
        };
    }

    /// <summary>
    /// Creates document links for all linked user parties.
    /// </summary>
    /// <param name="request">The document-link creation request.</param>
    /// <param name="cancellationToken">The token used to cancel database staging.</param>
    /// <returns>A task that completes when links are staged.</returns>
    private async Task CreateDocumentLinksAsync(
        KycDocumentLinkCreationModel request,
        CancellationToken cancellationToken)
    {
        // Link every submitted identity scan to each current party context owned by the user.
        var links = request.Parties.SelectMany(party =>
        {
            var partyLinks = new List<DocumentLink>
            {
                new()
                {
                    Document = request.FrontDocument,
                    EntityTypeId = request.MasterData.EntityType.Id,
                    EntityId = party.Id,
                    LinkTypeId = request.MasterData.FrontLinkType.Id,
                    IsPrimary = true,
                    SortOrder = KYC_FRONT_DOCUMENT_SORT_ORDER,
                    StatusId = request.MasterData.LinkStatus.Id
                }
            };
            if (request.BackDocument is not null && request.MasterData.BackLinkType is not null)
            {
                partyLinks.Add(new DocumentLink
                {
                    Document = request.BackDocument,
                    EntityTypeId = request.MasterData.EntityType.Id,
                    EntityId = party.Id,
                    LinkTypeId = request.MasterData.BackLinkType.Id,
                    IsPrimary = false,
                    SortOrder = KYC_BACK_DOCUMENT_SORT_ORDER,
                    StatusId = request.MasterData.LinkStatus.Id
                });
            }

            return partyLinks;
        });

        // Stage all document links in one repository call for the surrounding transaction.
        await _kycRepositories.DocumentLinkRepository.AddRangeAsync(links, cancellationToken);
    }

    /// <summary>
    /// Ensures the user does not already have a pending or approved identity review across linked contexts.
    /// </summary>
    /// <param name="request">The KYC submission guard request.</param>
    /// <param name="cancellationToken">The token used to cancel database reads.</param>
    /// <returns>A task that completes when the account can submit KYC.</returns>
    private async Task EnsureKycCanBeSubmittedAsync(
        KycSubmissionGuardModel request,
        CancellationToken cancellationToken)
    {
        // KYC status is shared across user contexts, so inspect all linked party identifiers.
        var partyIds = request.Parties.Select(x => x.Id).ToList();
        var existingIdentifiers = await _kycRepositories.PartyIdentifierRepository.GetTrackedByPartyIdsAndTypesAsync(
            partyIds,
            request.IdentifierTypeIds,
            cancellationToken);

        // KYC is shared by the identity user, so pending or approved data in any party context blocks re-upload.
        if (existingIdentifiers.Any(x => x.StatusId != request.RejectedStatusId))
        {
            _logger.LogWarning(
                InfrastructureLogConstants.UserLogs.KYC_REUPLOAD_BLOCKED,
                request.CurrentUserPublicId);

            throw new ApiException(
                ApplicationErrorConstants.KycErrors.KYC_REUPLOAD_NOT_ALLOWED_MESSAGE,
                ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID);
        }
    }

    /// <summary>
    /// Creates or updates the canonical identity identifier used by the current user's KYC submission.
    /// </summary>
    /// <param name="parties">The tracked linked parties for the user.</param>
    /// <param name="request">The KYC submission payload.</param>
    /// <param name="masterData">The resolved KYC master-data values.</param>
    /// <param name="cancellationToken">The token used to cancel database reads.</param>
    /// <returns>A task that completes when the identifier data is staged.</returns>
    private async Task UpsertPartyIdentifierAsync(
        IReadOnlyCollection<Party> parties,
        SubmitKycRequestDto request,
        KycMasterDataContextModel masterData,
        CancellationToken cancellationToken)
    {
        // The submitted identifier value is the uniqueness key across all party contexts.
        var identifierValue = request.IdentifierValue;
        var partyIds = parties.Select(x => x.Id).ToHashSet();
        var identifier = await _kycRepositories.PartyIdentifierRepository.GetByTypeAndValueAsync(
            masterData.IdentifierType.Id,
            identifierValue,
            cancellationToken);

        if (identifier is not null && !partyIds.Contains(identifier.PartyId))
        {
            throw new ApiException(
                ApplicationErrorConstants.KycErrors.KYC_IDENTIFIER_ALREADY_USED_MESSAGE,
                ApplicationErrorConstants.KycErrorCodes.AUTH_KYC_INVALID);
        }

        if (identifier is null)
        {
            // Create the canonical identifier on the first linked party; document links cover all parties.
            identifier = new PartyIdentifier
            {
                PartyId = parties.First().Id,
                IdentifierTypeId = masterData.IdentifierType.Id,
                IdentifierValue = identifierValue
            };
            await _kycRepositories.PartyIdentifierRepository.AddAsync(identifier, cancellationToken);
        }

        // Submit KYC only stages document data and pending status; profile sync happens after approval.
        request.Adapt(identifier);
        identifier.GenderOnDocumentId = masterData.Gender.Id;
        identifier.StatusId = masterData.KycStatus.Id;
    }

    /// <summary>
    /// Creates a missing party context and makes it current for the user.
    /// </summary>
    /// <param name="user">The tracked current user entity.</param>
    /// <param name="targetPartyType">The target party type master-data value.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that completes when the new context is persisted.</returns>
    private async Task<long> CreatePartyContextAsync(
        User user,
        string targetPartyType,
        CancellationToken cancellationToken)
    {
        // Resolve party status and type in one batch before creating the new context.
        var masterDataValues = await _repositories.MasterDataValueRepository.GetByTypeAndValuesAsync(
            [
                new MasterDataValueKeyModel(PARTY_STATUS_TYPE, ACTIVE_STATUS),
                new MasterDataValueKeyModel(PARTY_TYPE_TYPE, targetPartyType)
            ],
            cancellationToken);
        var partyStatus = MasterDataValueHelper.GetRequired(
            masterDataValues,
            new MasterDataValueRequirementModel(
                PARTY_STATUS_TYPE,
                ACTIVE_STATUS,
                ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_FORBIDDEN_OPERATION));
        var partyType = MasterDataValueHelper.GetRequired(
            masterDataValues,
            new MasterDataValueRequirementModel(
                PARTY_TYPE_TYPE,
                targetPartyType,
                ApplicationErrorConstants.ContextErrors.REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_FORBIDDEN_OPERATION));

        // Stage the first Party of the requested type; the caller transaction persists the relation and session together.
        var party = new Party
        {
            PartyTypeId = partyType.Id,
            DisplayName = string.Format(
                PARTY_CONTEXT_DISPLAY_NAME_SUFFIX_FORMAT,
                user.FullName,
                partyType.Name),
            PrimaryEmail = user.Email,
            PrimaryPhone = user.PhoneNumber,
            StatusId = partyStatus.Id
        };

        await _repositories.PartyRepository.AddAsync(party, cancellationToken);
        await _repositories.UserPartyRepository.AddAsync(new UserParty { User = user, Party = party }, cancellationToken);

        return party.Id;
    }

    /// <summary>
    /// Builds the external-provider response list from configured providers and linked providers.
    /// </summary>
    /// <param name="linkedProviders">The providers currently linked by the user-info query.</param>
    /// <returns>The normalized provider list with linked state only.</returns>
    private List<ExternalProviderResponseDto> BuildExternalProviderResponses(
        IEnumerable<ExternalProviderResponseDto> linkedProviders)
    {
        // Build a lookup from linked providers so configured providers can be merged with link state.
        var linkedLookup = (linkedProviders ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Provider))
            .GroupBy(x => x.Provider, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
        var supportedProviders = _externalAuthenticationOptions.Providers
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => x.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Return every supported provider plus any persisted linked provider not currently in config.
        return supportedProviders
            .Concat(linkedLookup.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(providerName =>
            {
                var response = providerName.Adapt<ExternalProviderResponseDto>();
                response.IsLinked = linkedLookup.ContainsKey(providerName);
                return response;
            })
            .ToList();
    }
}
