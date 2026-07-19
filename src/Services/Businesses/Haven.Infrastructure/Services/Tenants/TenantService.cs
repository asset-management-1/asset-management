namespace Haven.Infrastructure.Services.Tenants;

/// <summary>
/// Provides tenant list, detail, creation, move-out, and QR join workflows.
/// </summary>
public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IMasterDataService _masterDataService;
    private readonly ICachingService _cachingService;
    private readonly IDistributedLockService _distributedLockService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TenantService> _logger;

    /// <summary>
    /// Creates the tenant service.
    /// </summary>
    /// <param name="tenantRepository">The tenant repository.</param>
    /// <param name="unitRepository">The unit repository used for room-scoped aggregate operations.</param>
    /// <param name="masterDataService">The master-data service.</param>
    /// <param name="cachingService">The cache service used for transient QR join tokens.</param>
    /// <param name="distributedLockService">The distributed lock used to serialize room join capacity checks.</param>
    /// <param name="unitOfWork">The unit of work used for transactional writes.</param>
    /// <param name="logger">The service logger.</param>
    public TenantService(
        ITenantRepository tenantRepository,
        IUnitRepository unitRepository,
        IMasterDataService masterDataService,
        ICachingService cachingService,
        IDistributedLockService distributedLockService,
        IUnitOfWork unitOfWork,
        ILogger<TenantService> logger)
    {
        _tenantRepository = tenantRepository;
        _unitRepository = unitRepository;
        _masterDataService = masterDataService;
        _cachingService = cachingService;
        _distributedLockService = distributedLockService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Loads a paged tenant ledger scoped to the current landlord party.
    /// </summary>
    /// <param name="request">The tenant list request.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The paged tenant list response.</returns>
    public async Task<PaginationResponse<IReadOnlyList<TenantListItemResponseDto>>> GetTenantsAsync(
        TenantListRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Normalize request filters into the tenant repository's Dapper parameter model.
        var parameters = request.Adapt<TenantListQueryParametersModel>();
        var rows = await _tenantRepository.GetTenantPageAsync(parameters, cancellationToken);
        var total = rows.FirstOrDefault()?.TotalCount ?? 0;

        // Return a compact read model; tenant list composition stays flat enough for Mapster.
        _logger.LogInformation(
            InfrastructureLogConstants.TenantLogs.TENANTS_LOADED,
            rows.Count,
            request.CurrentParty.PartyPublicId);

        return new PaginationResponse<IReadOnlyList<TenantListItemResponseDto>>(
            rows.Adapt<IReadOnlyList<TenantListItemResponseDto>>(),
            request.PageNumber,
            request.PageSize,
            total);
    }

    /// <summary>
    /// Loads one tenant occupancy detail scoped to the current landlord party.
    /// </summary>
    /// <param name="request">The tenant detail request.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tenant detail response.</returns>
    public async Task<TenantDetailResponseDto> GetTenantDetailAsync(
        TenantDetailRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Detail reads are scoped by occupancy id plus the current landlord party.
        var row = await _tenantRepository.GetTenantDetailAsync(
            request.Adapt<TenantScopedQueryParametersModel>(),
            cancellationToken);

        if (row is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.TenantErrors.ERROR_TENANT_OCCUPANCY_NOT_FOUND,
                NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        // Dapper already returns the exact detail row shape, so mapping stays one-to-one.
        _logger.LogInformation(
            InfrastructureLogConstants.TenantLogs.TENANT_DETAIL_LOADED,
            request.OccupancyPublicId,
            request.CurrentParty.PartyPublicId);

        return row.Adapt<TenantDetailResponseDto>();
    }

    /// <summary>
    /// Adds a primary tenant or occupant to a room.
    /// </summary>
    /// <param name="request">The tenant creation request.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The created tenant detail response.</returns>
    public async Task<TenantDetailResponseDto> CreateTenantAsync(
        TenantCreateRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Resolve the stable lookup values before entering the room critical section.
        var lookups = await LoadTenantMasterDataAsync(cancellationToken);

        // Step 2: Serialize every occupancy mutation for the requested room across application instances.
        var lockHandle = await _distributedLockService.TryAcquireAsync(
            $"{ROOM_OCCUPANCY_LOCK_KEY_PREFIX}{request.RoomId:N}{ROOM_OCCUPANCY_LOCK_KEY_SUFFIX}",
            TimeSpan.FromSeconds(ROOM_OCCUPANCY_LOCK_LEASE_SECONDS),
            cancellationToken);

        if (lockHandle is null)
        {
            throw new ApiException(ApplicationErrorConstants.TenantErrors.ERROR_TENANT_JOIN_IN_PROGRESS, BAD_REQUEST);
        }

        await using (lockHandle)
        {
            // Step 3: Re-read scope and live occupancy guards inside the transaction protected by the room lock.
            return await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
            {
                var room = await GetScopedRoomAsync(
                    request.RoomId,
                    request.CurrentParty,
                    transactionToken);

                // The request may point to an existing tenant party, an account, or a manual profile.
                var tenantParty = await ResolveTenantPartyAsync(request, lookups, transactionToken);

                // Pending and active occupancy rows both block a duplicate room membership.
                await EnsureTenantNotInRoomAsync(room.Id, tenantParty.Id, lookups, transactionToken);

                // Step 4: Stage the contract and occupancy only after all live room guards pass.
                return await CreateTenantOccupancyAsync(
                    new TenantOccupancyCreationContextModel
                    {
                        Room = room,
                        LandlordParty = request.CurrentParty,
                        TenantParty = tenantParty,
                        RoleCode = request.RoleCode,
                        ContractStartDate = request.ContractStartDate,
                        ContractEndDate = request.ContractEndDate,
                        ContractRentAmount = request.ContractRentAmount,
                        DepositAmount = request.DepositAmount,
                        Lookups = lookups
                    },
                    transactionToken);
            }, cancellationToken);
        }
    }

    /// <summary>
    /// Moves one tenant occupancy out of a room.
    /// </summary>
    /// <param name="request">The tenant delete request.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The operation status response.</returns>
    public async Task<OperationStatusResponseDto> DeleteTenantAsync(
        TenantDeleteRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Move-out needs both current and target statuses before the transaction starts.
        var lookups = await LoadTenantMasterDataAsync(cancellationToken);
        var activeStatus = lookups.OccupancyStatusActive;
        var movedOutStatus = lookups.OccupancyStatusMovedOut;
        var availableStatus = lookups.UnitStatusAvailable;

        // Step 1: Resolve only the room identifier needed for lock selection without tracking stale occupancy state.
        var roomPublicId = await _tenantRepository.GetMoveOutRoomPublicIdAsync(
            request.OccupancyPublicId,
            request.CurrentParty.PartyId,
            activeStatus.Id,
            cancellationToken);

        if (!roomPublicId.HasValue)
        {
            throw new ApiException(
                ApplicationErrorConstants.TenantErrors.ERROR_TENANT_OCCUPANCY_NOT_FOUND,
                NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        // Step 2: Serialize move-out with landlord add and QR join for the same room.
        var lockHandle = await _distributedLockService.TryAcquireAsync(
            $"{ROOM_OCCUPANCY_LOCK_KEY_PREFIX}{roomPublicId.Value:N}{ROOM_OCCUPANCY_LOCK_KEY_SUFFIX}",
            TimeSpan.FromSeconds(ROOM_OCCUPANCY_LOCK_LEASE_SECONDS),
            cancellationToken);

        if (lockHandle is null)
        {
            throw new ApiException(ApplicationErrorConstants.TenantErrors.ERROR_TENANT_JOIN_IN_PROGRESS, BAD_REQUEST);
        }

        await using (lockHandle)
        {
            // Step 3: Re-read the active tracked occupancy and all move-out guards inside one transaction.
            await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
            {
                // Re-read inside the protected transaction so a repeated request cannot mutate stale tracked state.
                var occupancy = await _tenantRepository.GetOccupancyForMoveOutAsync(
                    request.OccupancyPublicId,
                    request.CurrentParty.PartyId,
                    activeStatus.Id,
                    transactionToken);

                if (occupancy is null)
                {
                    throw new ApiException(
                        ApplicationErrorConstants.TenantErrors.ERROR_TENANT_OCCUPANCY_NOT_FOUND,
                        NOT_FOUND,
                        StatusCodes.Status404NotFound);
                }

                // Count current active occupants only after the lock and transaction establish a stable room snapshot.
                var activeCount = await _tenantRepository.CountActiveOccupanciesAsync(
                    occupancy.UnitId,
                    activeStatus.Id,
                    transactionToken);

                // Mark the occupancy as moved out and clean room-level vehicle links for that tenant.
                occupancy.StatusId = movedOutStatus.Id;
                occupancy.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);

                await _tenantRepository.SoftDeleteVehiclesAsync(
                    occupancy.UnitId,
                    occupancy.PartyId,
                    transactionToken);

                if (activeCount <= 1)
                {
                    // Only touch room status when the room has no other active occupant after this move-out.
                    var room = await _unitRepository.GetRoomByIdAsync(occupancy.UnitId, transactionToken);

                    if (room is not null)
                    {
                        room.StatusId = availableStatus.Id;
                    }
                }

                _logger.LogInformation(
                    InfrastructureLogConstants.TenantLogs.TENANT_MOVED_OUT,
                    request.OccupancyPublicId,
                    request.CurrentParty.PartyPublicId);
            }, cancellationToken);
        }

        return OperationStatusResponseHelper.Success(
            ApplicationMessageConstants.TenantMessages.TENANT_DELETE_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Creates a temporary room tenant-join QR payload.
    /// </summary>
    /// <param name="request">The tenant join QR request.</param>
    /// <param name="cancellationToken">The token used to cancel the cache mutation.</param>
    /// <returns>The QR token response.</returns>
    public async Task<TenantJoinQrResponseDto> CreateTenantJoinQrAsync(
        TenantJoinQrRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Validate landlord access to the room before creating a cache-backed join token.
        var room = await GetScopedRoomAsync(request.RoomPublicId, request.CurrentParty, cancellationToken);
        var roleCode = request.RoleCode.ToUpperInvariant();

        if (!string.Equals(roleCode, TENANT_ROLE_PRIMARY, StringComparison.Ordinal)
            && !string.Equals(roleCode, TENANT_ROLE_OCCUPANT, StringComparison.Ordinal))
        {
            throw new ApiException(
                ApplicationErrorConstants.TenantErrors.ERROR_TENANT_ROLE_INVALID,
                BAD_REQUEST,
                StatusCodes.Status400BadRequest);
        }

        // Cache only the room, landlord scope, and role; TTL is the source of truth for expiry.
        var token = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        var payload = new TenantJoinPayloadModel
        {
            RoomPublicId = room.PublicId,
            LandlordPartyPublicId = request.CurrentParty.PartyPublicId,
            RoleCode = roleCode,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(TENANT_JOIN_TOKEN_EXPIRY_MINUTES)
        };

        await _cachingService.SetAbsoluteAsync(
            $"{TENANT_JOIN_CACHE_KEY_PREFIX}{token}",
            payload,
            TimeSpan.FromMinutes(TENANT_JOIN_TOKEN_EXPIRY_MINUTES),
            cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.TenantLogs.TENANT_JOIN_QR_CREATED,
            room.PublicId,
            request.CurrentParty.PartyPublicId);

        return new TenantJoinQrResponseDto
        {
            Token = token
        };
    }

    /// <summary>
    /// Loads a tenant join preview from a QR token.
    /// </summary>
    /// <param name="request">The tenant join preview request.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tenant join preview response.</returns>
    public async Task<TenantJoinPreviewResponseDto> GetTenantJoinPreviewAsync(
        TenantJoinPreviewRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Token preview loads only enough room context for the tenant confirmation screen.
        var payload = await GetValidTenantJoinPayloadAsync(request.Token, cancellationToken);
        var room = await _unitRepository.GetTenantJoinRoomAsync(
            new TenantJoinRoomQueryParametersModel
            {
                LandlordPartyPublicId = payload.LandlordPartyPublicId,
                RoomPublicId = payload.RoomPublicId,
                RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES
            },
            cancellationToken);

        if (room is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.TenantErrors.ERROR_TENANT_JOIN_TOKEN_INVALID,
                BAD_REQUEST,
                StatusCodes.Status400BadRequest);
        }

        return room.Adapt<TenantJoinPreviewResponseDto>();
    }

    /// <summary>
    /// Confirms a tenant join request from a valid QR token.
    /// </summary>
    /// <param name="request">The tenant join confirmation request.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The created tenant detail response.</returns>
    public async Task<TenantDetailResponseDto> ConfirmTenantJoinAsync(
        TenantJoinConfirmRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // A tenant scan can only be confirmed while the current party is an active tenant party.
        if (!string.Equals(
                request.CurrentParty.PartyTypeCode,
                MASTER_CODE_PARTY_TYPE_TENANT,
                StringComparison.OrdinalIgnoreCase)
            || !string.Equals(request.CurrentParty.StatusCode, MASTER_CODE_ACTIVE, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(
                ApplicationErrorConstants.TenantErrors.ERROR_TENANT_CONTEXT_REQUIRED,
                FORBIDDEN,
                StatusCodes.Status403Forbidden);
        }

        // Step 1: Read once to derive the room lock; the token is validated again after acquisition.
        var initialPayload = await GetValidTenantJoinPayloadAsync(request.Token, cancellationToken);
        var lockHandle = await _distributedLockService.TryAcquireAsync(
            $"{ROOM_OCCUPANCY_LOCK_KEY_PREFIX}{initialPayload.RoomPublicId:N}{ROOM_OCCUPANCY_LOCK_KEY_SUFFIX}",
            TimeSpan.FromSeconds(ROOM_OCCUPANCY_LOCK_LEASE_SECONDS),
            cancellationToken);

        if (lockHandle is null)
        {
            throw new ApiException(ApplicationErrorConstants.TenantErrors.ERROR_TENANT_JOIN_IN_PROGRESS, BAD_REQUEST);
        }

        await using (lockHandle)
        {
            // Step 2: Re-read the token inside the lock so an expired token cannot mutate the room.
            var payload = await GetValidTenantJoinPayloadAsync(request.Token, cancellationToken);

            // Resolve stable status/type IDs after the lock is held; scoped entities are re-read in the transaction.
            var lookups = await LoadTenantMasterDataAsync(cancellationToken);

            var normalizedRole = payload.RoleCode.ToUpperInvariant();
            var cacheKey = $"{TENANT_JOIN_CACHE_KEY_PREFIX}{request.Token}";

            // Step 3: Re-read room scope, tenant lifecycle, duplicate, and capacity state in the DB transaction.
            var response = await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
            {
                var room = await _unitRepository.GetTenantJoinRoomForMutationAsync(
                    new TenantJoinRoomQueryParametersModel
                    {
                        LandlordPartyPublicId = payload.LandlordPartyPublicId,
                        RoomPublicId = payload.RoomPublicId,
                        RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES
                    },
                    transactionToken);

                if (room is null)
                {
                    throw new ApiException(
                        ApplicationErrorConstants.TenantErrors.ERROR_TENANT_JOIN_TOKEN_INVALID,
                        BAD_REQUEST,
                        StatusCodes.Status400BadRequest);
                }

                var tenantParty = await _tenantRepository.GetTenantPartyAsync(
                    request.CurrentParty.PartyPublicId,
                    lookups.TenantPartyType.Id,
                    lookups.PartyActiveStatus.Id,
                    transactionToken);

                if (tenantParty is null)
                {
                    throw new ApiException(
                        ApplicationErrorConstants.TenantErrors.ERROR_TENANT_CONTEXT_REQUIRED,
                        FORBIDDEN,
                        StatusCodes.Status403Forbidden);
                }

                await EnsureTenantNotInRoomAsync(room.UnitId, tenantParty.Id, lookups, transactionToken);

                await ValidateTenantPlacementAsync(
                    room.UnitId,
                    room.RentalModeId,
                    room.BedCount,
                    normalizedRole,
                    lookups,
                    transactionToken);

                return await CreatePendingTenantJoinOccupancyAsync(
                    new TenantJoinPendingOccupancyContextModel
                    {
                        Room = room,
                        TenantParty = tenantParty,
                        RoleCode = normalizedRole,
                        Lookups = lookups
                    },
                    transactionToken);
            }, cancellationToken);

            _logger.LogInformation(
                InfrastructureLogConstants.TenantLogs.TENANT_CREATED,
                response.Id,
                response.Room.Id,
                payload.LandlordPartyPublicId);

            // Step 4: Consume the one-time token only after the database commit; cache failure cannot undo committed state.
            try
            {
                await _cachingService.RemoveAsync(cacheKey, CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    InfrastructureLogConstants.TenantLogs.TENANT_JOIN_TOKEN_REMOVE_FAILED,
                    response.Room.Id);
            }

            return response;
        }
    }

    /// <summary>
    /// Loads the master-data values required by tenant creation, move-out, and join flows.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The resolved master-data values.</returns>
    private async Task<TenantMasterDataContextModel> LoadTenantMasterDataAsync(
        CancellationToken cancellationToken)
    {
        // Tenant flows share the same status/type codes across add, move-out, and QR join paths.
        var keys = new HashSet<MasterDataKeyModel>
        {
            new(MasterDataTypeEnum.PartyType, MASTER_CODE_PARTY_TYPE_TENANT),
            new(MasterDataTypeEnum.PartyStatus, MASTER_CODE_ACTIVE),
            new(MasterDataTypeEnum.ContractType, MASTER_CODE_CONTRACT_TYPE_RENTAL),
            new(MasterDataTypeEnum.ContractSource, MASTER_CODE_CONTRACT_SOURCE_OFFLINE_UPLOAD),
            new(MasterDataTypeEnum.ContractStatus, MASTER_CODE_CONTRACT_STATUS_ACTIVE),
            new(MasterDataTypeEnum.OccupancyStatus, MASTER_CODE_OCCUPANCY_STATUS_ACTIVE),
            new(MasterDataTypeEnum.OccupancyStatus, MASTER_CODE_OCCUPANCY_STATUS_PENDING),
            new(MasterDataTypeEnum.OccupancyStatus, MASTER_CODE_OCCUPANCY_STATUS_MOVED_OUT),
            new(MasterDataTypeEnum.UnitRentalMode, MASTER_CODE_RENTAL_MODE_SHARED_BED),
            new(MasterDataTypeEnum.UnitStatus, MASTER_CODE_UNIT_STATUS_OCCUPIED),
            new(MasterDataTypeEnum.UnitStatus, MASTER_CODE_UNIT_STATUS_AVAILABLE)
        };

        // The master-data service validates the requested keys before these exact values are copied.
        var values = await _masterDataService.GetValuesAsync(keys, cancellationToken);

        return new TenantMasterDataContextModel
        {
            TenantPartyType = values[new MasterDataKeyModel(
                MasterDataTypeEnum.PartyType,
                MASTER_CODE_PARTY_TYPE_TENANT)],
            PartyActiveStatus = values[new MasterDataKeyModel(MasterDataTypeEnum.PartyStatus, MASTER_CODE_ACTIVE)],
            ContractTypeRental = values[new MasterDataKeyModel(
                MasterDataTypeEnum.ContractType,
                MASTER_CODE_CONTRACT_TYPE_RENTAL)],
            ContractSourceOfflineUpload = values[new MasterDataKeyModel(
                MasterDataTypeEnum.ContractSource,
                MASTER_CODE_CONTRACT_SOURCE_OFFLINE_UPLOAD)],
            ContractStatusActive = values[new MasterDataKeyModel(
                MasterDataTypeEnum.ContractStatus,
                MASTER_CODE_CONTRACT_STATUS_ACTIVE)],
            OccupancyStatusActive = values[new MasterDataKeyModel(
                MasterDataTypeEnum.OccupancyStatus,
                MASTER_CODE_OCCUPANCY_STATUS_ACTIVE)],
            OccupancyStatusPending = values[new MasterDataKeyModel(
                MasterDataTypeEnum.OccupancyStatus,
                MASTER_CODE_OCCUPANCY_STATUS_PENDING)],
            OccupancyStatusMovedOut = values[new MasterDataKeyModel(
                MasterDataTypeEnum.OccupancyStatus,
                MASTER_CODE_OCCUPANCY_STATUS_MOVED_OUT)],
            SharedBedRentalMode = values[new MasterDataKeyModel(
                MasterDataTypeEnum.UnitRentalMode,
                MASTER_CODE_RENTAL_MODE_SHARED_BED)],
            UnitStatusOccupied = values[new MasterDataKeyModel(
                MasterDataTypeEnum.UnitStatus,
                MASTER_CODE_UNIT_STATUS_OCCUPIED)],
            UnitStatusAvailable = values[new MasterDataKeyModel(
                MasterDataTypeEnum.UnitStatus,
                MASTER_CODE_UNIT_STATUS_AVAILABLE)]
        };
    }

    /// <summary>
    /// Loads one room graph under the current landlord party scope.
    /// </summary>
    /// <param name="roomPublicId">The room public identifier.</param>
    /// <param name="currentParty">The current landlord party context.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The scoped room graph.</returns>
    private async Task<Unit> GetScopedRoomAsync(
        Guid roomPublicId,
        CurrentPartyContextModel currentParty,
        CancellationToken cancellationToken)
    {
        // Room scope is enforced by repository SQL/EF criteria so services never trust the public id alone.
        var room = await _unitRepository.GetRoomForOccupancyMutationAsync(
            new RoomScopedQueryParametersModel
            {
                CurrentPartyId = currentParty.PartyId,
                RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES,
                RoomPublicId = roomPublicId,
                NoFurniturePackageTypeCode = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE
            },
            cancellationToken);

        return room ?? throw new ApiException(
            ApplicationErrorConstants.RoomErrors.ERROR_ROOM_NOT_FOUND,
            NOT_FOUND,
            StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Resolves or creates the tenant party referenced by the add-tenant request.
    /// </summary>
    /// <param name="request">The add-tenant request.</param>
    /// <param name="lookups">The tenant master-data values used for party validation.</param>
    /// <param name="cancellationToken">The token used to cancel repository operations.</param>
    /// <returns>The tenant party entity.</returns>
    private async Task<Party> ResolveTenantPartyAsync(
        TenantCreateRequestModel request,
        TenantMasterDataContextModel lookups,
        CancellationToken cancellationToken)
    {
        // Tenant creation always resolves a tenant-type active party, regardless of input source.
        var tenantType = lookups.TenantPartyType;
        var activeStatus = lookups.PartyActiveStatus;

        if (request.TenantId.HasValue)
        {
            // Existing tenant id means FE already selected the business party to attach.
            var tenant = await _tenantRepository.GetTenantPartyAsync(
                request.TenantId.Value,
                tenantType.Id,
                activeStatus.Id,
                cancellationToken);

            return tenant ?? throw new ApiException(
                ApplicationErrorConstants.TenantErrors.ERROR_TENANT_NOT_FOUND,
                NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        if (request.TenantAccountId.HasValue)
        {
            // Account id can map to zero, one, or many tenant parties because users may hold multiple roles.
            return await ResolveTenantPartyFromAccountAsync(
                request.TenantAccountId.Value,
                tenantType.Id,
                activeStatus.Id,
                cancellationToken);
        }

        // Manual profile creates an unlinked tenant party for occupants without an account.
        return CreateTenantParty(request.TenantProfile, tenantType.Id, activeStatus.Id);
    }

    /// <summary>
    /// Resolves the tenant party for an existing account or creates one when none exists.
    /// </summary>
    /// <param name="tenantAccountId">The account public identifier.</param>
    /// <param name="tenantTypeId">The tenant party-type identifier.</param>
    /// <param name="activeStatusId">The active party-status identifier.</param>
    /// <param name="cancellationToken">The token used to cancel repository operations.</param>
    /// <returns>The tenant party entity.</returns>
    private async Task<Party> ResolveTenantPartyFromAccountAsync(
        Guid tenantAccountId,
        long tenantTypeId,
        long activeStatusId,
        CancellationToken cancellationToken)
    {
        // Load the account with tenant-party links so multi-role ambiguity can be handled in one place.
        var user = await _tenantRepository.GetUserWithTenantPartiesAsync(
            tenantAccountId,
            cancellationToken);

        if (user is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.TenantErrors.ERROR_TENANT_NOT_FOUND,
                NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        // Only active tenant parties are valid candidates for room occupancy.
        var tenantParties = user.UserParties
            .Where(link => link.Party is not null
                           && link.Party.PartyTypeId == tenantTypeId
                           && link.Party.StatusId == activeStatusId
                           && !link.Party.IsDeleted)
            .Select(link => link.Party)
            .ToList();

        if (tenantParties.Count == 1)
        {
            return tenantParties[0];
        }

        if (tenantParties.Count > 1)
        {
            // Multiple tenant parties require a separate FE selection flow; do not expose candidate diagnostics here.
            throw new ApiException(
                ApplicationErrorConstants.TenantErrors.ERROR_TENANT_ACCOUNT_MULTIPLE_PARTIES,
                BAD_REQUEST);
        }

        // No tenant party exists yet, so create the tenant party and link it to the selected account.
        var party = new Party
        {
            PublicId = Guid.NewGuid(),
            PartyTypeId = tenantTypeId,
            DisplayName = user.FullName,
            PrimaryPhone = user.PhoneNumber,
            PrimaryEmail = user.Email,
            StatusId = activeStatusId
        };
        var userParty = new UserParty
        {
            PublicId = Guid.NewGuid(),
            User = user,
            Party = party
        };

        await _tenantRepository.AddTenantPartyLinkAsync(party, userParty, cancellationToken);

        return party;
    }

    /// <summary>
    /// Creates an unlinked tenant party from manually entered tenant profile data.
    /// </summary>
    /// <param name="profile">The tenant profile payload.</param>
    /// <param name="tenantTypeId">The tenant party-type identifier.</param>
    /// <param name="activeStatusId">The active party-status identifier.</param>
    /// <returns>The new tenant party entity.</returns>
    private static Party CreateTenantParty(
        TenantProfileModel profile,
        long tenantTypeId,
        long activeStatusId)
    {
        // Mapster copies profile text as submitted; identity and master-data IDs remain backend-owned.
        var party = profile.Adapt<Party>();
        party.PublicId = Guid.NewGuid();
        party.PartyTypeId = tenantTypeId;
        party.StatusId = activeStatusId;
        return party;
    }

    /// <summary>
    /// Creates the contract when needed and attaches a tenant occupancy to the room.
    /// </summary>
    /// <param name="context">The tenant occupancy creation context.</param>
    /// <param name="cancellationToken">The token used to cancel the staged mutation.</param>
    /// <returns>The created tenant detail response.</returns>
    private async Task<TenantDetailResponseDto> CreateTenantOccupancyAsync(
        TenantOccupancyCreationContextModel context,
        CancellationToken cancellationToken)
    {
        // Landlord-created tenants can be primary contract holders or non-contract occupants.
        var normalizedRole = context.RoleCode.ToUpperInvariant();

        if (!string.Equals(normalizedRole, TENANT_ROLE_PRIMARY, StringComparison.Ordinal)
            && !string.Equals(normalizedRole, TENANT_ROLE_OCCUPANT, StringComparison.Ordinal))
        {
            throw new ApiException(
                ApplicationErrorConstants.TenantErrors.ERROR_TENANT_ROLE_INVALID,
                BAD_REQUEST,
                StatusCodes.Status400BadRequest);
        }

        // Recheck role-specific capacity in the caller-owned transaction before staging a contract or occupancy row.
        await ValidateTenantPlacementAsync(
            context.Room.Id,
            context.Room.RentalModeId,
            context.Room.BedCount,
            normalizedRole,
            context.Lookups,
            cancellationToken);

        // Contract dates default only for landlord-driven creation, not tenant QR pending joins.
        var startDate = DateOnly.FromDateTime(context.ContractStartDate ?? DateTime.UtcNow);
        var endDate = context.ContractEndDate.HasValue
            ? DateOnly.FromDateTime(context.ContractEndDate.Value)
            : (DateOnly?)null;
        var activeOccupancyStatus = context.Lookups.OccupancyStatusActive;
        var occupiedUnitStatus = context.Lookups.UnitStatusOccupied;
        Contract contract = null;

        // Primary tenant creation owns the rental contract; occupants attach without a contract.
        if (string.Equals(normalizedRole, TENANT_ROLE_PRIMARY, StringComparison.Ordinal))
        {
            // Create a contract only for the primary tenant who signs for this room.
            contract = await BuildContractAsync(
                new TenantContractCreationContextModel
                {
                    Room = context.Room,
                    LandlordPartyId = context.LandlordParty.PartyId,
                    TenantPartyId = context.TenantParty.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    RentAmount = context.ContractRentAmount,
                    DepositAmount = context.DepositAmount,
                    Lookups = context.Lookups
                },
                cancellationToken);
        }

        // The occupancy is the room membership record for both primary tenants and occupants.
        var occupancy = new Occupancy
        {
            PublicId = Guid.NewGuid(),
            Contract = contract,
            Party = context.TenantParty,
            PropertyId = context.Room.PropertyId,
            UnitId = context.Room.Id,
            StartDate = startDate,
            EndDate = endDate,
            StatusId = activeOccupancyStatus.Id,
            IsPrimaryTenant = string.Equals(normalizedRole, TENANT_ROLE_PRIMARY, StringComparison.Ordinal)
        };

        // Landlord-created occupancy makes the room occupied immediately.
        context.Room.StatusId = occupiedUnitStatus.Id;
        await _tenantRepository.AddTenantOccupancyAsync(occupancy, contract, cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.TenantLogs.TENANT_CREATED,
            occupancy.PublicId,
            context.Room.PublicId,
            context.LandlordParty.PartyPublicId);

        // The caller commits the staged rows before exposing this response.
        return BuildTenantResponse(
            context.Room,
            context.TenantParty,
            occupancy,
            contract,
            normalizedRole,
            activeOccupancyStatus);
    }

    /// <summary>
    /// Creates a pending occupancy when a tenant scans and confirms a room QR token.
    /// </summary>
    /// <param name="context">The room, tenant, lookup, and token state for this pending join.</param>
    /// <param name="cancellationToken">The token used to cancel the staged mutation.</param>
    /// <returns>The pending tenant detail response.</returns>
    private async Task<TenantDetailResponseDto> CreatePendingTenantJoinOccupancyAsync(
        TenantJoinPendingOccupancyContextModel context,
        CancellationToken cancellationToken)
    {
        var room = context.Room;
        var pendingStatus = context.Lookups.OccupancyStatusPending;

        // QR joins stage pending membership only; the caller owns transaction commit and post-commit token cleanup.
        var occupancy = new Occupancy
        {
            PublicId = Guid.NewGuid(),
            Party = context.TenantParty,
            PropertyId = room.PropertyId,
            UnitId = room.UnitId,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            StatusId = pendingStatus.Id,
            IsPrimaryTenant = string.Equals(context.RoleCode, TENANT_ROLE_PRIMARY, StringComparison.Ordinal)
        };

        await _tenantRepository.AddTenantOccupancyAsync(occupancy, null, cancellationToken);

        return new TenantDetailResponseDto
        {
            Id = occupancy.PublicId,
            TenantId = context.TenantParty.PublicId,
            Tenant = context.TenantParty.DisplayName,
            Phone = context.TenantParty.PrimaryPhone,
            Email = context.TenantParty.PrimaryEmail,
            RoleCode = context.RoleCode,
            Property = room.Adapt<TenantPropertySummaryResponseDto>(),
            Room = room.Adapt<TenantRoomSummaryResponseDto>(),
            OccupancyStatusCode = pendingStatus.Code,
            OccupancyStatusName = pendingStatus.Name
        };
    }

    /// <summary>
    /// Rejects a landlord or QR join when the tenant already reserves or occupies the room.
    /// </summary>
    /// <param name="unitId">The internal room identifier.</param>
    /// <param name="tenantPartyId">The internal tenant party identifier.</param>
    /// <param name="lookups">The tenant master-data values containing placement statuses.</param>
    /// <param name="cancellationToken">The token used to cancel the repository read.</param>
    private async Task EnsureTenantNotInRoomAsync(
        long unitId,
        long tenantPartyId,
        TenantMasterDataContextModel lookups,
        CancellationToken cancellationToken)
    {
        // Pending requests reserve the room just like active occupancies for duplicate detection.
        var placementStatusIds = new[]
        {
            lookups.OccupancyStatusPending.Id,
            lookups.OccupancyStatusActive.Id
        };
        if (await _tenantRepository.HasOccupancyAsync(
                unitId,
                tenantPartyId,
                placementStatusIds,
                cancellationToken))
        {
            throw new ApiException(ApplicationErrorConstants.TenantErrors.ERROR_TENANT_ALREADY_IN_ROOM, BAD_REQUEST);
        }
    }

    /// <summary>
    /// Validates primary-tenant capacity rules for the target room.
    /// </summary>
    /// <param name="unitId">The internal room identifier.</param>
    /// <param name="rentalModeId">The room rental-mode identifier.</param>
    /// <param name="bedCount">The configured shared-bed capacity.</param>
    /// <param name="roleCode">The normalized tenant role.</param>
    /// <param name="lookups">The tenant master-data values used for room capacity checks.</param>
    /// <param name="cancellationToken">The token used to cancel repository reads.</param>
    private async Task ValidateTenantPlacementAsync(
        long unitId,
        long rentalModeId,
        int? bedCount,
        string roleCode,
        TenantMasterDataContextModel lookups,
        CancellationToken cancellationToken)
    {
        // Only primary tenants are limited by contract/capacity rules; members can share a room record.
        if (!string.Equals(roleCode, TENANT_ROLE_PRIMARY, StringComparison.Ordinal))
        {
            return;
        }

        // Pending requests reserve capacity alongside active primary occupancies.
        var primaryCount = await _tenantRepository.CountPrimaryOccupanciesAsync(
            unitId,
            [lookups.OccupancyStatusPending.Id, lookups.OccupancyStatusActive.Id],
            cancellationToken);

        // Shared-bed rooms allow one primary tenant per bed; whole-room modes allow only one primary.
        var sharedBedMode = lookups.SharedBedRentalMode;

        if (rentalModeId == sharedBedMode.Id)
        {
            var totalBeds = bedCount ?? 0;

            if (primaryCount >= totalBeds)
            {
                throw new ApiException(
                    ApplicationErrorConstants.TenantErrors.ERROR_ROOM_BED_CAPACITY_EXCEEDED,
                    BAD_REQUEST);
            }

            return;
        }

        if (primaryCount > 0)
        {
            throw new ApiException(
                ApplicationErrorConstants.TenantErrors.ERROR_ROOM_PRIMARY_TENANT_EXISTS,
                BAD_REQUEST);
        }
    }

    /// <summary>
    /// Builds the rental contract entity for a primary tenant.
    /// </summary>
    /// <param name="context">The tenant contract creation context.</param>
    /// <param name="cancellationToken">The token used to cancel uniqueness checks.</param>
    /// <returns>The new contract entity.</returns>
    private async Task<Contract> BuildContractAsync(
        TenantContractCreationContextModel context,
        CancellationToken cancellationToken)
    {
        // Contract codes stay repository-unique because the generated numeric segment is public-facing.
        var contractCode = await CodeGenerationHelper.GenerateUniqueCodeAsync(
            new UniqueCodeGenerationOptions
            {
                Prefix = CONTRACT_CODE_PREFIX,
                RandomLength = CONTRACT_CODE_RANDOM_LENGTH,
                MaxAttempts = CONTRACT_CODE_MAX_ATTEMPTS,
                ExistsAsync = _tenantRepository.ContractCodeExistsAsync
            },
            cancellationToken);

        // The contract captures the primary rental relationship for landlord-created primary tenants.
        return new Contract
        {
            PublicId = Guid.NewGuid(),
            ContractCode = contractCode,
            ContractTypeId = context.Lookups.ContractTypeRental.Id,
            ContractSourceId = context.Lookups.ContractSourceOfflineUpload.Id,
            PropertyId = context.Room.PropertyId,
            UnitId = context.Room.Id,
            PrimaryPartyId = context.LandlordPartyId,
            SecondaryPartyId = context.TenantPartyId,
            EffectiveFrom = context.StartDate,
            EffectiveTo = context.EndDate,
            RentAmount = context.RentAmount ?? context.Room.BaseRentAmount,
            DepositAmount = context.DepositAmount ?? context.Room.DefaultDepositAmount,
            StatusId = context.Lookups.ContractStatusActive.Id
        };
    }

    /// <summary>
    /// Builds a tenant detail response from newly persisted tenant state.
    /// </summary>
    /// <param name="room">The scoped room graph.</param>
    /// <param name="tenantParty">The tenant party.</param>
    /// <param name="occupancy">The created occupancy.</param>
    /// <param name="contract">The optional created contract.</param>
    /// <param name="roleCode">The normalized tenant role.</param>
    /// <param name="occupancyStatus">The occupancy status to expose in the response.</param>
    /// <returns>The tenant detail response.</returns>
    private static TenantDetailResponseDto BuildTenantResponse(
        Unit room,
        Party tenantParty,
        Occupancy occupancy,
        Contract contract,
        string roleCode,
        MasterDataValueModel occupancyStatus)
    {
        // Return the same response shape for contract and non-contract occupants; missing data stays null.
        return new TenantDetailResponseDto
        {
            Id = occupancy.PublicId,
            TenantId = tenantParty.PublicId,
            Tenant = tenantParty.DisplayName,
            Phone = tenantParty.PrimaryPhone,
            Email = tenantParty.PrimaryEmail,
            RoleCode = roleCode,
            Property = room.Property.Adapt<TenantPropertySummaryResponseDto>(),
            Room = room.Adapt<TenantRoomSummaryResponseDto>(),
            ContractStartDate = contract?.EffectiveFrom.ToDateTime(TimeOnly.MinValue),
            ContractEndDate = contract?.EffectiveTo?.ToDateTime(TimeOnly.MinValue),
            ContractRentAmount = contract?.RentAmount,
            OccupancyStatusCode = occupancyStatus.Code,
            OccupancyStatusName = occupancyStatus.Name
        };
    }

    /// <summary>
    /// Loads and validates a cached tenant join payload.
    /// </summary>
    /// <param name="token">The QR join token.</param>
    /// <param name="cancellationToken">The token used to cancel the cache read.</param>
    /// <returns>The valid tenant join payload.</returns>
    private async Task<TenantJoinPayloadModel> GetValidTenantJoinPayloadAsync(
        string token,
        CancellationToken cancellationToken)
    {
        // Cache TTL owns expiry; missing payload means the token is invalid or expired.
        var payload = await _cachingService.GetAsync<TenantJoinPayloadModel>(
            $"{TENANT_JOIN_CACHE_KEY_PREFIX}{token}",
            cancellationToken);

        if (payload is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.TenantErrors.ERROR_TENANT_JOIN_TOKEN_INVALID,
                BAD_REQUEST,
                StatusCodes.Status400BadRequest);
        }

        return payload;
    }
}
