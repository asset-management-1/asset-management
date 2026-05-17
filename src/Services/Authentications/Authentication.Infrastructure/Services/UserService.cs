namespace Authentication.Infrastructure.Services;

/// <summary>
/// Provides infrastructure operations for current-user profile and context flows.
/// </summary>
public class UserService : IUserService
{
    private readonly AuthenticationRepositoryDependencies _repositories;
    private readonly AuthenticationKycRepositoryDependencies _kycRepositories;
    private readonly IPartyVehicleRepository _partyVehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IObjectStorageService _objectStorageService;
    private readonly ExternalAuthenticationOptions _externalAuthenticationOptions;
    private readonly R2StorageOptions _r2StorageOptions;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// Creates the current-user service with profile, context, KYC, vehicle, and storage dependencies.
    /// </summary>
    /// <param name="repositories">The grouped authentication repositories used by profile and context operations.</param>
    /// <param name="kycRepositories">The grouped repositories used by KYC and document persistence.</param>
    /// <param name="partyVehicleRepository">The repository used for tenant profile vehicle flows.</param>
    /// <param name="unitOfWork">The unit of work used for transactional writes.</param>
    /// <param name="authService">The current authenticated-principal accessor.</param>
    /// <param name="supportDependencies">The grouped storage and option dependencies for user flows.</param>
    /// <param name="logger">The user-service logger.</param>
    public UserService(
        AuthenticationRepositoryDependencies repositories,
        AuthenticationKycRepositoryDependencies kycRepositories,
        IPartyVehicleRepository partyVehicleRepository,
        IUnitOfWork unitOfWork,
        IAuthService authService,
        UserServiceSupportDependencies supportDependencies,
        ILogger<UserService> logger)
    {
        _repositories = repositories;
        _kycRepositories = kycRepositories;
        _partyVehicleRepository = partyVehicleRepository;
        _unitOfWork = unitOfWork;
        _authService = authService;
        _objectStorageService = supportDependencies.ObjectStorageService;
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
        // Resolve current identity from the authenticated principal before loading account data.
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      ApplicationConstants.UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);
        var response = await _repositories.UserRepository.GetUserInfoResponseByPublicIdAsync(
            currentUserPublicId,
            cancellationToken);
        if (response is null)
        {
            throw new HttpStatusCodeException(
                ApplicationConstants.UNAUTHORIZED_REQUEST_MESSAGE,
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
    /// Loads registered vehicles for the current tenant profile.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The active vehicles registered under the current tenant party.</returns>
    public async Task<IReadOnlyList<UserVehicleResponseDto>> GetVehiclesAsync(
        CancellationToken cancellationToken = default)
    {
        // Resolve the active tenant party before querying tenant-scoped profile vehicles.
        var user = await LoadTrackedCurrentUserAsync(cancellationToken);
        var tenantParty = await LoadCurrentTenantPartyContextAsync(user, cancellationToken);

        // Profile vehicles belong to the active tenant party only, not every party linked to the user.
        var vehicles = await _partyVehicleRepository.GetActiveByPartyIdAsync(
            tenantParty.PartyId,
            cancellationToken);
        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.USER_VEHICLES_LOADED,
            user.PublicId,
            vehicles.Count);

        return vehicles;
    }

    /// <summary>
    /// Registers a vehicle under the current tenant profile.
    /// </summary>
    /// <param name="request">The tenant vehicle registration payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The registered vehicle response.</returns>
    public async Task<UserVehicleResponseDto> RegisterVehicleAsync(
        RegisterUserVehicleRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await LoadTrackedCurrentUserAsync(cancellationToken);
        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.USER_VEHICLE_REGISTRATION_STARTED,
            user.PublicId);

        // Tenant vehicle registration is scoped to the user's currently selected tenant party.
        var vehicleContext = await ResolveVehicleRegistrationContextAsync(user, request, cancellationToken);
        var normalizedLicensePlate = request.LicensePlate.NormalizeAlphanumericCode();
        if (string.IsNullOrWhiteSpace(normalizedLicensePlate))
        {
            throw new ApiException(INVALID_LICENSE_PLATE_MESSAGE, AUTH_VEHICLE_INVALID);
        }

        // The active plate check is tenant-party scoped, so another tenant may use the same plate independently.
        if (await _partyVehicleRepository.ActivePlateExistsAsync(
                vehicleContext.TenantPartyId,
                normalizedLicensePlate,
                cancellationToken))
        {
            _logger.LogWarning(
                InfrastructureLogConstants.UserLogs.USER_VEHICLE_DUPLICATE_PLATE_BLOCKED,
                user.PublicId);

            throw new ApiException(VEHICLE_LICENSE_PLATE_ALREADY_EXISTS_MESSAGE, AUTH_VEHICLE_INVALID);
        }

        // Vehicle type is profile metadata; parking fees remain a later billing policy per rental place.
        // Images are uploaded before the row is saved, so persisted image URLs always point to successful uploads.
        var uploadedImages = await UploadVehicleImagesAsync(user.PublicId, request, cancellationToken);
        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.USER_VEHICLE_UPLOAD_COMPLETED,
            user.PublicId,
            uploadedImages.Front is not null,
            uploadedImages.Side is not null);

        var vehicle = new PartyVehicle
        {
            PartyId = vehicleContext.TenantPartyId,
            VehicleTypeId = vehicleContext.VehicleType.Id,
            VehicleName = request.VehicleName,
            LicensePlate = request.LicensePlate,
            NormalizedLicensePlate = normalizedLicensePlate,
            FrontImageUrl = ObjectStorageHelper.BuildObjectUrl(
                _r2StorageOptions.PublicBaseUrl,
                uploadedImages.Front?.ObjectKey),
            SideImageUrl = ObjectStorageHelper.BuildObjectUrl(
                _r2StorageOptions.PublicBaseUrl,
                uploadedImages.Side?.ObjectKey)
        };

        try
        {
            // Persist the vehicle only after duplicate validation and optional image uploads succeed.
            await _partyVehicleRepository.AddAsync(vehicle, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                InfrastructureLogConstants.UserLogs.USER_VEHICLE_PERSISTENCE_FAILED,
                user.PublicId);

            await CleanupUploadedObjectsAsync(
                user.PublicId,
                uploadedImages.UploadedObjects,
                cancellationToken);

            throw new ApiException(
                VEHICLE_REGISTRATION_FAILED_MESSAGE,
                AUTH_VEHICLE_INVALID,
                StatusCodes.Status500InternalServerError);
        }
        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.USER_VEHICLE_REGISTERED,
            user.PublicId);

        vehicle.VehicleType = vehicleContext.VehicleType;
        return vehicle.Adapt<UserVehicleResponseDto>();
    }

    /// <summary>
    /// Soft deletes a vehicle from the current tenant profile.
    /// </summary>
    /// <param name="vehiclePublicId">The public vehicle identifier to remove.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The delete operation result.</returns>
    public async Task<OperationStatusResponseDto> DeleteVehicleAsync(
        Guid vehiclePublicId,
        CancellationToken cancellationToken = default)
    {
        // Resolve the current tenant party before loading the owned vehicle row.
        var user = await LoadTrackedCurrentUserAsync(cancellationToken);
        var tenantParty = await LoadCurrentTenantPartyContextAsync(user, cancellationToken);
        var vehicle = await _partyVehicleRepository.GetTrackedByPublicIdAndPartyIdAsync(
                          vehiclePublicId,
                          tenantParty.PartyId,
                          cancellationToken)
                      ?? throw new ApiException(
                          VEHICLE_NOT_FOUND_MESSAGE,
                          AUTH_VEHICLE_NOT_FOUND,
                          StatusCodes.Status404NotFound);

        // Ownership is enforced in the lookup above; only the active tenant party can remove this row.
        vehicle.IsDeleted = true;
        await _partyVehicleRepository.UpdateAsync(vehicle);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.USER_VEHICLE_DELETED,
            user.PublicId);

        return OperationStatusResponseHelper.Success(VEHICLE_DELETED_SUCCESS_MESSAGE);
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
        var normalizedPhoneNumber = request.PhoneNumber;

        // Phone number uniqueness is checked only when the submitted value changes.
        if (!string.IsNullOrWhiteSpace(normalizedPhoneNumber)
            && !string.Equals(user.PhoneNumber, normalizedPhoneNumber, StringComparison.OrdinalIgnoreCase)
            && await _repositories.UserRepository.PhoneNumberExistsAsync(normalizedPhoneNumber, cancellationToken))
        {
            throw new ApiException(PHONE_NUMBER_ALREADY_EXISTS_MESSAGE, AUTH_USER_ALREADY_EXISTS);
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
        ApplyProfileUpdate(user, request, gender, normalizedPhoneNumber, avatarUrl);
        SyncPartyProfile(parties, request.FullName, normalizedPhoneNumber, null);

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
                USER_INFO_UPDATE_FAILED_MESSAGE,
                AUTH_PROFILE_UPDATE_FAILED,
                StatusCodes.Status500InternalServerError);
        }
        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.USER_INFO_UPDATED,
            user.PublicId);

        return OperationStatusResponseHelper.Success(USER_INFO_UPDATED_SUCCESS_MESSAGE);
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
            throw new ApiException(EMAIL_ALREADY_EXISTS_MESSAGE, AUTH_USER_ALREADY_EXISTS);
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
        // Load the tracked user after OTP verification so the verified email can be persisted.
        var user = await _repositories.UserRepository.GetTrackedByPublicIdAsync(currentUserPublicId, cancellationToken)
                   ?? throw new HttpStatusCodeException(
                       ApplicationConstants.UNAUTHORIZED_REQUEST_MESSAGE,
                       UNAUTHORIZED,
                       StatusCodes.Status401Unauthorized);
        var normalizedNewEmail = request.NewEmail;
        var oldEmail = user.Email;

        // Re-check uniqueness at commit time to protect against races during the OTP window.
        await EnsureEmailAvailableAsync(normalizedNewEmail, user.Id, cancellationToken);

        // Update identity email fields and keep username aligned only when it was email-based.
        user.Email = normalizedNewEmail;
        user.EmailConfirmed = true;
        if (string.Equals(user.UserName, oldEmail, StringComparison.OrdinalIgnoreCase))
        {
            user.UserName = normalizedNewEmail;
        }

        // Party contact snapshots follow the verified email so user-info stays consistent across contexts.
        SyncPartyProfile(await LoadTrackedUserPartiesAsync(user, cancellationToken), null, null, normalizedNewEmail);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.CHANGE_EMAIL_VERIFIED,
            currentUserPublicId);

        return OperationStatusResponseHelper.Success(CHANGE_EMAIL_COMPLETED_SUCCESS_MESSAGE);
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
        // Load the current user and parties because KYC is shared across tenant/landlord contexts.
        var user = await LoadTrackedCurrentUserAsync(cancellationToken);
        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.KYC_SUBMISSION_STARTED,
            user.PublicId);

        var parties = (await LoadTrackedUserPartiesAsync(user, cancellationToken)).ToList();
        if (parties.Count == 0)
        {
            throw new ApiException(
                REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                AUTH_KYC_INVALID,
                StatusCodes.Status500InternalServerError);
        }

        // KYC lookup values are resolved in one batch because this flow needs many master-data records.
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
            parties,
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
                    // Persist document metadata and link every submitted scan to each party context owned by the user.
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
                            Parties = parties,
                            FrontDocument = frontDocument,
                            BackDocument = backDocument,
                            MasterData = kycMasterData
                        },
                        ct);

                    // Manual review owns the final profile sync; submit only stages scanned data and private files.
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
                KYC_SUBMISSION_FAILED_MESSAGE,
                AUTH_KYC_INVALID,
                StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation(
            InfrastructureLogConstants.UserLogs.KYC_SUBMITTED,
            user.PublicId);

        return new KycSubmissionResponseDto
        {
            IsSubmitted = true,
            Status = KycStatusEnum.Pending,
            Message = KYC_SUBMITTED_SUCCESS_MESSAGE
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
        // Resolve the requested UI context into the persisted party type used by user-party mappings.
        var targetContext = request.TargetContext;
        var user = await LoadTrackedCurrentUserAsync(cancellationToken);
        var targetPartyType = AuthenticationFlowHelper.ToMasterDataPartyTypeValue(targetContext);
        var existingUserParty = await _repositories.UserPartyRepository.GetByUserIdAndPartyTypeAsync(
            user.Id,
            targetPartyType,
            cancellationToken);

        if (existingUserParty is null)
        {
            // First switch into a missing UI context creates a new party and user-party link.
            await ActivateNewContextAsync(user, targetPartyType, cancellationToken);
            _logger.LogInformation(
                InfrastructureLogConstants.ContextLogs.NEW_CONTEXT_CREATED,
                user.PublicId);
        }
        else
        {
            // Reusing an existing user-party link only changes the active CurrentPartyId.
            await ActivateExistingContextAsync(user, existingUserParty.PartyId, cancellationToken);
            _logger.LogInformation(
                InfrastructureLogConstants.ContextLogs.EXISTING_CONTEXT_ACTIVATED,
                user.PublicId);
        }

        // Rebuild available contexts after the switch so the response reflects newly created contexts.
        var availableContexts = (await _repositories.UserPartyRepository.GetActiveByUserIdAsync(user.Id, cancellationToken))
            .Select(x => AuthenticationFlowHelper.ToPartyContextValue(x.Party?.PartyType?.Code ?? x.Party?.PartyType?.Name))
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
            user.PublicId);

        return new SwitchPartyResponseDto
        {
            IsSuccess = true,
            Message = SWITCH_PARTY_SUCCESS_MESSAGE,
            CurrentContext = ApiEnumContractMapper.ToPartyType(targetContext)
                             ?? throw new InvalidOperationException(
                                 string.Format(UNSUPPORTED_PARTY_CONTEXT_VALUE_MESSAGE, targetContext)),
            AvailableContexts = ApiEnumContractMapper.ToPartyTypes(availableContexts)
        };
    }

    /// <summary>
    /// Loads the current authenticated user as a tracked entity.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked current user.</returns>
    private async Task<User> LoadTrackedCurrentUserAsync(CancellationToken cancellationToken)
    {
        // The auth handler has already validated the token; here we only need the normalized public user id.
        var currentUserPublicId = _authService.UserId();
        if (!currentUserPublicId.HasValue)
        {
            _logger.LogWarning(InfrastructureLogConstants.UserLogs.CURRENT_USER_CLAIM_MISSING);

            throw new HttpStatusCodeException(
                ApplicationConstants.UNAUTHORIZED_REQUEST_MESSAGE,
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
            ApplicationConstants.UNAUTHORIZED_REQUEST_MESSAGE,
            UNAUTHORIZED,
            StatusCodes.Status401Unauthorized);
    }

    /// <summary>
    /// Loads the active party context and ensures it is a tenant profile.
    /// </summary>
    /// <param name="user">The tracked current user entity.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The current tenant party context.</returns>
    private async Task<CurrentPartyContextModel> LoadCurrentTenantPartyContextAsync(
        User user,
        CancellationToken cancellationToken)
    {
        // Vehicle registration is tenant-only, so a missing active party blocks the operation.
        if (!user.CurrentPartyId.HasValue)
        {
            _logger.LogWarning(
                InfrastructureLogConstants.UserLogs.TENANT_CONTEXT_MISSING,
                user.PublicId);

            throw new HttpStatusCodeException(
                TENANT_CONTEXT_REQUIRED_MESSAGE,
                AUTH_FORBIDDEN_OPERATION,
                StatusCodes.Status403Forbidden);
        }

        // Load the current party type without tracking because this method only validates context.
        var currentParty = await _repositories.PartyRepository.GetContextByIdAsync(
            user.CurrentPartyId.Value,
            cancellationToken);

        // The party type was normalized when saved, so tenant validation uses the current party row directly.
        var tenantPartyType = PartyTypeEnum.Tenant.ToMasterDataCode();
        if (currentParty is null
            || (currentParty.PartyTypeCode != tenantPartyType
                && currentParty.PartyTypeName != tenantPartyType))
        {
            _logger.LogWarning(
                InfrastructureLogConstants.UserLogs.TENANT_CONTEXT_REJECTED,
                user.PublicId,
                user.CurrentPartyId.Value);

            throw new HttpStatusCodeException(
                TENANT_CONTEXT_REQUIRED_MESSAGE,
                AUTH_FORBIDDEN_OPERATION,
                StatusCodes.Status403Forbidden);
        }

        return currentParty;
    }

    /// <summary>
    /// Resolves the active tenant party and requested vehicle type for vehicle registration.
    /// </summary>
    /// <param name="user">The tracked current user entity.</param>
    /// <param name="request">The vehicle registration payload.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The resolved tenant party and vehicle type context.</returns>
    private async Task<VehicleRegistrationContextModel> ResolveVehicleRegistrationContextAsync(
        User user,
        RegisterUserVehicleRequestDto request,
        CancellationToken cancellationToken)
    {
        // Validate the current context before resolving vehicle metadata.
        var currentParty = await LoadCurrentTenantPartyContextAsync(user, cancellationToken);
        var vehicleType = await _repositories.MasterDataValueRepository.GetByTypeAndValueAsync(
                              VEHICLE_TYPE_TYPE,
                              request.VehicleType,
                              cancellationToken)
                          ?? throw new ApiException(INVALID_VEHICLE_TYPE_MESSAGE, AUTH_VEHICLE_INVALID);

        // Vehicle type is the only request-driven master-data lookup left in this flow.
        return new VehicleRegistrationContextModel
        {
            TenantPartyId = currentParty.PartyId,
            VehicleType = vehicleType
        };
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
        if (user.CurrentPartyId.HasValue && !partyIds.Contains(user.CurrentPartyId.Value))
        {
            // Include CurrentPartyId defensively even if a legacy mapping row is missing.
            partyIds.Add(user.CurrentPartyId.Value);
        }

        // Return tracked parties because callers synchronize profile/contact snapshots.
        return await _repositories.PartyRepository.GetTrackedByIdsAsync(partyIds, cancellationToken);
    }

    /// <summary>
    /// Applies optional profile fields to the tracked user entity.
    /// </summary>
    /// <param name="user">The tracked current user entity.</param>
    /// <param name="request">The profile update payload.</param>
    /// <param name="gender">The resolved gender master data, if supplied.</param>
    /// <param name="normalizedPhoneNumber">The Mapster-normalized phone number, if supplied.</param>
    /// <param name="avatarUrl">The backend-uploaded avatar URL, if supplied.</param>
    private static void ApplyProfileUpdate(
        User user,
        UpdateUserInfoRequestDto request,
        MasterDataValue gender,
        string normalizedPhoneNumber,
        string avatarUrl)
    {
        // Apply only supplied optional fields so omitted profile values remain unchanged.
        if (request.FullName is not null)
        {
            user.FullName = request.FullName;
        }

        if (normalizedPhoneNumber is not null)
        {
            user.PhoneNumber = normalizedPhoneNumber;
        }

        if (avatarUrl is not null)
        {
            user.AvatarUrl = avatarUrl;
        }

        if (request.DateOfBirth.HasValue)
        {
            user.DateOfBirth = request.DateOfBirth;
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
            throw new ApiException(EMAIL_ALREADY_EXISTS_MESSAGE, AUTH_USER_ALREADY_EXISTS);
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
               ?? throw new ApiException(INVALID_GENDER_MESSAGE, AUTH_INVALID_GENDER);
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
        var lookups = new List<MasterDataValueLookupModel>
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
            lookups.Add(new MasterDataValueLookupModel(DOCUMENT_LINK_TYPE_TYPE, backLinkType));
        }

        var masterDataValues = await _repositories.MasterDataValueRepository.GetByTypeAndValuesAsync(lookups, cancellationToken);

        // KYC needs several master values; resolve them from one batched DB lookup to avoid repeated round-trips.
        return new KycMasterDataContextModel
        {
            IdentifierType = MasterDataLookupHelper.GetRequired(
                masterDataValues,
                new MasterDataRequiredLookupModel(
                    IDENTIFIER_TYPE_TYPE,
                    request.IdentifierType,
                    REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    AUTH_KYC_INVALID)),
            SupportedIdentifierTypes =
            [
                MasterDataLookupHelper.GetRequired(
                    masterDataValues,
                    new MasterDataRequiredLookupModel(
                        IDENTIFIER_TYPE_TYPE,
                        cccdIdentifierType,
                        REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                        AUTH_KYC_INVALID)),
                MasterDataLookupHelper.GetRequired(
                    masterDataValues,
                    new MasterDataRequiredLookupModel(
                        IDENTIFIER_TYPE_TYPE,
                        passportIdentifierType,
                        REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                        AUTH_KYC_INVALID))
            ],
            Gender = MasterDataLookupHelper.GetRequired(
                masterDataValues,
                new MasterDataRequiredLookupModel(
                    PROFILE_GENDER_TYPE,
                    request.GenderOnDocument,
                    INVALID_GENDER_MESSAGE,
                    AUTH_INVALID_GENDER)),
            DocumentType = MasterDataLookupHelper.GetRequired(
                masterDataValues,
                new MasterDataRequiredLookupModel(
                    DOCUMENT_TYPE_TYPE,
                    NATIONAL_ID_DOCUMENT_TYPE,
                    REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    AUTH_KYC_INVALID)),
            StorageProvider = MasterDataLookupHelper.GetRequired(
                masterDataValues,
                new MasterDataRequiredLookupModel(
                    STORAGE_PROVIDER_TYPE,
                    R2_STORAGE_PROVIDER,
                    REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    AUTH_KYC_INVALID)),
            DocumentStatus = MasterDataLookupHelper.GetRequired(
                masterDataValues,
                new MasterDataRequiredLookupModel(
                    DOCUMENT_STATUS_TYPE,
                    UPLOADED_DOCUMENT_STATUS,
                    REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    AUTH_KYC_INVALID)),
            EntityType = MasterDataLookupHelper.GetRequired(
                masterDataValues,
                new MasterDataRequiredLookupModel(
                    ENTITY_TYPE_TYPE,
                    PARTY_ENTITY_TYPE,
                    REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    AUTH_KYC_INVALID)),
            FrontLinkType = MasterDataLookupHelper.GetRequired(
                masterDataValues,
                new MasterDataRequiredLookupModel(
                    DOCUMENT_LINK_TYPE_TYPE,
                    frontLinkType,
                    REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    AUTH_KYC_INVALID)),
            BackLinkType = string.IsNullOrWhiteSpace(backLinkType)
                ? null
                : MasterDataLookupHelper.GetRequired(
                    masterDataValues,
                    new MasterDataRequiredLookupModel(
                        DOCUMENT_LINK_TYPE_TYPE,
                        backLinkType,
                        REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                        AUTH_KYC_INVALID)),
            LinkStatus = MasterDataLookupHelper.GetRequired(
                masterDataValues,
                new MasterDataRequiredLookupModel(
                    DOCUMENT_LINK_STATUS_TYPE,
                    ACTIVE_DOCUMENT_LINK_STATUS,
                    REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    AUTH_KYC_INVALID)),
            KycStatus = MasterDataLookupHelper.GetRequired(
                masterDataValues,
                new MasterDataRequiredLookupModel(
                    KYC_STATUS_TYPE,
                    pendingKycStatus,
                    REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    AUTH_KYC_INVALID)),
            RejectedKycStatus = MasterDataLookupHelper.GetRequired(
                masterDataValues,
                new MasterDataRequiredLookupModel(
                    KYC_STATUS_TYPE,
                    rejectedKycStatus,
                    REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                    AUTH_KYC_INVALID))
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

            // Delegate document scans to the shared batch uploader with private KYC prefixing.
            return await ObjectStorageHelper.UploadOwnerScopedFormFilesAsync(
                _objectStorageService,
                new ObjectStorageUploadBatchRequestModel
                {
                    ConfiguredPrefix = _r2StorageOptions.KycObjectPrefix,
                    DefaultPrefix = DEFAULT_KYC_OBJECT_PREFIX,
                    OwnerPublicId = currentUserPublicId,
                    Files = files
                },
                uploadResult => new KycUploadedDocumentsModel
                {
                    Front = uploadResult.GetUpload(FRONT_OBJECT_SLOT),
                    Back = uploadResult.GetUpload(BACK_OBJECT_SLOT)
                },
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                InfrastructureLogConstants.UserLogs.KYC_UPLOAD_FAILED,
                currentUserPublicId);

            throw new ApiException(
                KYC_SUBMISSION_FAILED_MESSAGE,
                AUTH_KYC_UPLOAD_FAILED,
                StatusCodes.Status503ServiceUnavailable);
        }
    }

    /// <summary>
    /// Uploads optional tenant vehicle images to object storage.
    /// </summary>
    /// <param name="currentUserPublicId">The current user's public identifier.</param>
    /// <param name="request">The vehicle registration payload.</param>
    /// <param name="cancellationToken">The token used to cancel uploads.</param>
    /// <returns>The uploaded vehicle image metadata keyed by image slot.</returns>
    private async Task<VehicleUploadedImagesModel> UploadVehicleImagesAsync(
        Guid currentUserPublicId,
        RegisterUserVehicleRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Delegate optional vehicle images to the shared batch uploader with vehicle prefixing.
            return await ObjectStorageHelper.UploadOwnerScopedFormFilesAsync(
                _objectStorageService,
                new ObjectStorageUploadBatchRequestModel
                {
                    ConfiguredPrefix = _r2StorageOptions.VehicleObjectPrefix,
                    DefaultPrefix = DEFAULT_VEHICLE_OBJECT_PREFIX,
                    OwnerPublicId = currentUserPublicId,
                    Files =
                    [
                        new ObjectStorageUploadFileModel
                        {
                            SlotName = FRONT_OBJECT_SLOT,
                            ObjectTag = FRONT_OBJECT_SLOT,
                            File = request.FrontFile
                        },
                        new ObjectStorageUploadFileModel
                        {
                            SlotName = SIDE_OBJECT_SLOT,
                            ObjectTag = SIDE_OBJECT_SLOT,
                            File = request.SideFile
                        }
                    ]
                },
                uploadResult => new VehicleUploadedImagesModel
                {
                    Front = uploadResult.GetUpload(FRONT_OBJECT_SLOT),
                    Side = uploadResult.GetUpload(SIDE_OBJECT_SLOT)
                },
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                InfrastructureLogConstants.UserLogs.USER_VEHICLE_UPLOAD_FAILED,
                currentUserPublicId);

            throw new ApiException(
                VEHICLE_UPLOAD_FAILED_MESSAGE,
                AUTH_VEHICLE_UPLOAD_FAILED,
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
            // Delegate avatar upload to the shared owner-scoped batch uploader so object-key rules stay consistent.
            var uploadResult = await ObjectStorageHelper.UploadOwnerScopedFormFilesAsync(
                _objectStorageService,
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
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                InfrastructureLogConstants.UserLogs.USER_AVATAR_UPLOAD_FAILED,
                currentUserPublicId);

            throw new ApiException(
                AVATAR_UPLOAD_FAILED_MESSAGE,
                AUTH_PROFILE_UPLOAD_FAILED,
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
            upload => upload.ObjectKey,
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
            FileExtension = Path.GetExtension(request.File.FileName),
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

        // KYC is shared by the identity user, so any tenant/landlord context with pending/approved data blocks re-upload.
        if (existingIdentifiers.Any(x => x.StatusId != request.RejectedStatusId))
        {
            _logger.LogWarning(
                InfrastructureLogConstants.UserLogs.KYC_REUPLOAD_BLOCKED,
                request.CurrentUserPublicId);

            throw new ApiException(KYC_REUPLOAD_NOT_ALLOWED_MESSAGE, AUTH_KYC_INVALID);
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
        // The normalized identifier is the uniqueness key across all party contexts.
        var normalizedIdentifierValue = request.IdentifierValue;
        var partyIds = parties.Select(x => x.Id).ToHashSet();
        var identifier = await _kycRepositories.PartyIdentifierRepository.GetByTypeAndValueAsync(
            masterData.IdentifierType.Id,
            normalizedIdentifierValue,
            cancellationToken);

        if (identifier is not null && !partyIds.Contains(identifier.PartyId))
        {
            throw new ApiException(KYC_IDENTIFIER_ALREADY_USED_MESSAGE, AUTH_KYC_INVALID);
        }

        if (identifier is null)
        {
            // Create the canonical identifier on the first linked party; document links cover all parties.
            identifier = new PartyIdentifier
            {
                PartyId = parties.First().Id,
                IdentifierTypeId = masterData.IdentifierType.Id,
                IdentifierValue = normalizedIdentifierValue
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
    private async Task ActivateNewContextAsync(
        User user,
        string targetPartyType,
        CancellationToken cancellationToken)
    {
        // Resolve party status and type in one batch before creating the new context.
        var masterDataValues = await _repositories.MasterDataValueRepository.GetByTypeAndValuesAsync(
            [
                new MasterDataValueLookupModel(PARTY_STATUS_TYPE, ACTIVE_STATUS),
                new MasterDataValueLookupModel(PARTY_TYPE_TYPE, targetPartyType)
            ],
            cancellationToken);
        var partyStatus = MasterDataLookupHelper.GetRequired(
            masterDataValues,
            new MasterDataRequiredLookupModel(
                PARTY_STATUS_TYPE,
                ACTIVE_STATUS,
                REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                AUTH_FORBIDDEN_OPERATION));
        var partyType = MasterDataLookupHelper.GetRequired(
            masterDataValues,
            new MasterDataRequiredLookupModel(
                PARTY_TYPE_TYPE,
                targetPartyType,
                REQUIRED_MASTER_DATA_NOT_FOUND_MESSAGE,
                AUTH_FORBIDDEN_OPERATION));

        // Create the missing party context, link it to the user, and make it current atomically.
        await _unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                var party = new Party
                {
                    PartyTypeId = partyType.Id,
                    DisplayName = string.Format(PARTY_CONTEXT_DISPLAY_NAME_SUFFIX_FORMAT, user.FullName, partyType.Name),
                    PrimaryEmail = user.Email,
                    PrimaryPhone = user.PhoneNumber,
                    StatusId = partyStatus.Id
                };

                await _repositories.PartyRepository.AddAsync(party, ct);
                user.CurrentParty = party;
                await _repositories.UserRepository.UpdateAsync(user);
                await _repositories.UserPartyRepository.AddAsync(new UserParty { User = user, Party = party }, ct);
            },
            cancellationToken);
    }

    /// <summary>
    /// Makes an existing user-party context current for the user.
    /// </summary>
    /// <param name="user">The tracked current user entity.</param>
    /// <param name="partyId">The party identifier to activate.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that completes when the existing context is persisted.</returns>
    private async Task ActivateExistingContextAsync(
        User user,
        long partyId,
        CancellationToken cancellationToken = default)
    {
        // Existing contexts only need CurrentPartyId changed on the tracked user row.
        user.CurrentPartyId = partyId;
        await _repositories.UserRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
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
