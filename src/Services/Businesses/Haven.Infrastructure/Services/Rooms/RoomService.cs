namespace Haven.Infrastructure.Services.Rooms;

/// <summary>
/// Provides room list, detail, update, and delete workflows.
/// </summary>
public class RoomService : IRoomService
{
    private readonly IUnitRepository _unitRepository;
    private readonly IRentalChargePolicyRepository _rentalChargePolicyRepository;
    private readonly IUnitPackageRepository _unitPackageRepository;
    private readonly IUnitPackageItemRepository _unitPackageItemRepository;
    private readonly IMasterDataService _masterDataService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RoomService> _logger;

    /// <summary>
    /// Creates the room service.
    /// </summary>
    /// <param name="unitRepository">The unit repository.</param>
    /// <param name="rentalChargePolicyRepository">The room charge-policy repository.</param>
    /// <param name="unitPackageRepository">The unit package repository.</param>
    /// <param name="unitPackageItemRepository">The unit package item repository.</param>
    /// <param name="masterDataService">The master-data service.</param>
    /// <param name="unitOfWork">The unit of work used for transactional writes.</param>
    /// <param name="logger">The service logger.</param>
    public RoomService(
        IUnitRepository unitRepository,
        IRentalChargePolicyRepository rentalChargePolicyRepository,
        IUnitPackageRepository unitPackageRepository,
        IUnitPackageItemRepository unitPackageItemRepository,
        IMasterDataService masterDataService,
        IUnitOfWork unitOfWork,
        ILogger<RoomService> logger)
    {
        _unitRepository = unitRepository;
        _rentalChargePolicyRepository = rentalChargePolicyRepository;
        _unitPackageRepository = unitPackageRepository;
        _unitPackageItemRepository = unitPackageItemRepository;
        _masterDataService = masterDataService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Loads a paged room ledger scoped to the current landlord party.
    /// </summary>
    /// <param name="request">The room list request.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The paged room ledger grouped by property and floor.</returns>
    public async Task<PaginationResponse<IReadOnlyList<RoomListPropertyResponseDto>>> GetRoomsAsync(
        RoomListRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Dapper loads the current page as flat rows with grouping metadata.
        var parameters = request.Adapt<RoomListQueryParametersModel>();
        var roomRows = await _unitRepository.GetRoomPageAsync(parameters, cancellationToken);
        var total = roomRows.FirstOrDefault()?.TotalCount ?? 0;

        var tenantRows = await _unitRepository.GetRoomTenantsAsync(
            roomRows.Select(row => row.UnitPublicId).Distinct().ToArray(),
            cancellationToken);

        var response = new PaginationResponse<IReadOnlyList<RoomListPropertyResponseDto>>(
            RoomResponseMapper.MapList(roomRows, tenantRows),
            request.PageNumber,
            request.PageSize,
            total);

        _logger.LogInformation(
            InfrastructureLogConstants.RoomLogs.ROOMS_LOADED,
            roomRows.Count,
            request.CurrentParty.PartyPublicId);

        return response;
    }

    /// <summary>
    /// Loads room detail within the current landlord party's scope.
    /// </summary>
    /// <param name="request">The room detail request.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The room detail response.</returns>
    public async Task<RoomDetailResponseDto> GetRoomDetailAsync(
        RoomDetailRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Detail lookup is party-scoped; missing and unauthorized rooms both resolve as 404.
        var parameters = request.Adapt<RoomScopedQueryParametersModel>();
        var roomRow = await _unitRepository.GetRoomDetailAsync(parameters, cancellationToken);

        if (roomRow is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.RoomErrors.ERROR_ROOM_NOT_FOUND,
                NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        // Tenant, vehicle, and package collections have dedicated room-scoped APIs; detail loads only effective policies.
        var chargePolicies = await _unitRepository.GetEffectiveChargePoliciesAsync(
            request.RoomPublicId,
            cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.RoomLogs.ROOM_DETAIL_LOADED,
            request.RoomPublicId,
            request.CurrentParty.PartyPublicId);

        return RoomResponseMapper.MapDetail(roomRow, chargePolicies);
    }

    /// <summary>
    /// Updates a room and its override data.
    /// </summary>
    /// <param name="request">The room update request.</param>
    /// <param name="cancellationToken">The token used to cancel the room mutation.</param>
    /// <returns>The refreshed room detail response.</returns>
    public async Task<RoomDetailResponseDto> UpdateRoomAsync(
        RoomUpdateRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Resolve only master-data codes submitted by this partial update.
        var masterData = await _masterDataService.GetValuesAsync(
            GetUpdateMasterDataKeys(request),
            cancellationToken);

        // A single scoped tracked query verifies access and loads the graph needed by the mutation.
        var room = await _unitRepository.GetRoomGraphAsync(
                       request.Adapt<RoomScopedQueryParametersModel>(),
                       cancellationToken)
                   ?? throw new ApiException(
                       ApplicationErrorConstants.RoomErrors.ERROR_ROOM_NOT_FOUND,
                       NOT_FOUND,
                       StatusCodes.Status404NotFound);
        var roomFields = request.Adapt<RoomFieldUpdateModel>();

        // Translate submitted room type and rental-mode codes into persistence IDs before the shared field mapper runs.
        var roomLookups = MasterDataLookupMapper.BuildRoomFieldLookups(
            roomFields,
            masterData,
            includeAvailableStatus: false);

        // Prepare lookup-backed policy and package mutations before dependency guards or EF changes run.
        var chargePolicyMutations = (request.ChargePolicies ?? [])
            .Select(policy => new RoomChargePolicyMutationModel
            {
                Policy = policy,
                Lookups = MasterDataLookupMapper.BuildChargePolicyLookups(
                    policy,
                    masterData,
                    includeActiveStatus: !policy.Id.HasValue)
            })
            .ToList();
        var hasNewPackage = request.Packages?.Any(package => !package.Id.HasValue) == true;
        var packageLookups = string.Equals(
            request.PackageMode,
            ROOM_OVERRIDE_MODE_CUSTOM,
            StringComparison.OrdinalIgnoreCase)
            ? MasterDataLookupMapper.BuildUnitPackageLookups(masterData, hasNewPackage)
            : null;

        // Protected fields are checked against current dependencies before any tracked entity is mutated.
        var guard = (await _unitRepository.GetRoomMutationGuardsAsync(
                [request.RoomPublicId],
                cancellationToken))
            .FirstOrDefault() ?? new UnitMutationGuardModel { UnitPublicId = request.RoomPublicId };
        if (guard.HasDeleteBlockingDependencies
            && RoomFieldMutationMapper.HasProtectedFieldChanges(room, roomFields, roomLookups))
        {
            throw new ApiException(ApplicationErrorConstants.RoomErrors.ERROR_ROOM_MUTATION_BLOCKED, BAD_REQUEST);
        }

        await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
        {
            // Submitted room fields are applied through the shared Unit mapper used by room and property updates.
            RoomFieldMutationMapper.Apply(room, roomFields, roomLookups);

            if (!string.IsNullOrWhiteSpace(request.ChargePolicyMode))
            {
                // Switch between fallback-to-common and room-specific policy synchronization according to the submitted mode.
                await SyncChargePoliciesAsync(
                    room,
                    request.ChargePolicyMode,
                    chargePolicyMutations,
                    transactionToken);
            }

            if (!string.IsNullOrWhiteSpace(request.PackageMode))
            {
                // Switch between fallback-to-common and room-specific package synchronization according to the submitted mode.
                await SyncPackagesAsync(
                    room,
                    request.PackageMode,
                    request.Packages,
                    packageLookups,
                    transactionToken);
            }

            // Keep property-level structure counters aligned after a room edit changes floor or room state.
            room.Property.RecomputeStructureTotals();
            await _unitRepository.UpdateAsync(room);

            _logger.LogInformation(
                InfrastructureLogConstants.RoomLogs.ROOM_UPDATED,
                request.RoomPublicId,
                request.CurrentParty.PartyPublicId);
        }, cancellationToken);

        return await GetRoomDetailAsync(
            new RoomDetailRequestModel
            {
                CurrentParty = request.CurrentParty,
                RoomPublicId = request.RoomPublicId
            },
            cancellationToken);
    }

    /// <summary>
    /// Soft-deletes one room when no active dependency blocks deletion.
    /// </summary>
    /// <param name="request">The room delete request.</param>
    /// <param name="cancellationToken">The token used to cancel the room deletion.</param>
    /// <returns>The operation status response.</returns>
    public async Task<OperationStatusResponseDto> DeleteRoomAsync(
        RoomDeleteRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // A single scoped tracked query verifies access and loads the graph needed by deletion.
        var room = await _unitRepository.GetRoomGraphAsync(
                       request.Adapt<RoomScopedQueryParametersModel>(),
                       cancellationToken)
                   ?? throw new ApiException(
                       ApplicationErrorConstants.RoomErrors.ERROR_ROOM_NOT_FOUND,
                       NOT_FOUND,
                       StatusCodes.Status404NotFound);
        var guard = (await _unitRepository.GetRoomMutationGuardsAsync(
                [request.RoomPublicId],
                cancellationToken))
            .FirstOrDefault() ?? new UnitMutationGuardModel { UnitPublicId = request.RoomPublicId };
        if (guard.HasDeleteBlockingDependencies)
        {
            throw new ApiException(ApplicationErrorConstants.RoomErrors.ERROR_ROOM_MUTATION_BLOCKED, BAD_REQUEST);
        }

        await _unitOfWork.ExecuteInTransactionAsync(async _ =>
        {
            // Room delete removes only room-owned override data; contracts/occupancies were checked before mutation.
            _unitPackageRepository.DeletePackagesWithItems(room.UnitPackages.ToList());
            await _rentalChargePolicyRepository.DeleteRangeAsync(room.RentalChargePolicies.ToList());

            // Keep the tracked parent counters aligned with the active room collection after soft delete.
            room.Property.Units.Remove(room);
            room.Property.RecomputeStructureTotals();
            await _unitRepository.DeleteAsync(room);

            _logger.LogInformation(
                InfrastructureLogConstants.RoomLogs.ROOM_DELETED,
                request.RoomPublicId,
                request.CurrentParty.PartyPublicId);
        }, cancellationToken);

        return OperationStatusResponseHelper.Success(
            ApplicationMessageConstants.RoomMessages.ROOM_DELETE_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Synchronizes room-level charge policy overrides or clears them for common mode.
    /// </summary>
    /// <param name="room">The tracked room graph.</param>
    /// <param name="mode">The submitted common or custom policy mode.</param>
    /// <param name="chargePolicies">The prepared custom policy mutations.</param>
    /// <param name="cancellationToken">The token used to cancel the staged policy mutation.</param>
    private async Task SyncChargePoliciesAsync(
        Unit room,
        string mode,
        IReadOnlyList<RoomChargePolicyMutationModel> chargePolicies,
        CancellationToken cancellationToken)
    {
        if (string.Equals(mode, ROOM_OVERRIDE_MODE_COMMON, StringComparison.OrdinalIgnoreCase))
        {
            // COMMON mode removes room-level overrides so reads fall back to property-level common setup.
            await _rentalChargePolicyRepository.DeleteRangeAsync(room.RentalChargePolicies.ToList());
            room.RentalChargePolicies.Clear();
            return;
        }

        var existingById = room.RentalChargePolicies.ToDictionary(policy => policy.PublicId);

        foreach (var mutation in chargePolicies)
        {
            var requestPolicy = mutation.Policy;

            if (requestPolicy.Id.HasValue)
            {
                // Existing custom policies are matched by public id.
                // Missing ids belong to another scope or stale UI data.
                if (!existingById.TryGetValue(requestPolicy.Id.Value, out var existingPolicy))
                {
                    throw new ApiException(
                        ApplicationErrorConstants.RoomErrors.ERROR_ROOM_CHARGE_POLICY_NOT_FOUND,
                        NOT_FOUND,
                        StatusCodes.Status404NotFound);
                }

                // Lookup-backed IDs are resolved before mapping so the mapper only applies persistence fields.
                existingPolicy.Property = room.Property;
                existingPolicy.Unit = room;
                RentalChargePolicyFieldMapper.Apply(existingPolicy, requestPolicy, mutation.Lookups);
                await _rentalChargePolicyRepository.UpdateAsync(existingPolicy);
                continue;
            }

            // New custom policies are room-owned overrides; omitted policies are left unchanged in partial update mode.
            var newPolicy = new RentalChargePolicy
            {
                PublicId = Guid.NewGuid(),
                Property = room.Property,
                Unit = room
            };
            RentalChargePolicyFieldMapper.Apply(newPolicy, requestPolicy, mutation.Lookups);
            room.RentalChargePolicies.Add(newPolicy);
            await _rentalChargePolicyRepository.AddAsync(newPolicy, cancellationToken);
        }
    }

    /// <summary>
    /// Synchronizes room-level package overrides or clears them for common mode.
    /// </summary>
    /// <param name="room">The tracked room graph.</param>
    /// <param name="mode">The submitted common or custom package mode.</param>
    /// <param name="packages">The submitted package rows.</param>
    /// <param name="lookups">The package lookup values required by custom mode.</param>
    /// <param name="cancellationToken">The token used to cancel the staged package mutation.</param>
    private async Task SyncPackagesAsync(
        Unit room,
        string mode,
        IReadOnlyList<UpdateRoomPackageRequestDto> packages,
        UnitPackageMutationLookupModel lookups,
        CancellationToken cancellationToken)
    {
        if (string.Equals(mode, ROOM_OVERRIDE_MODE_COMMON, StringComparison.OrdinalIgnoreCase))
        {
            // COMMON mode removes all room package overrides and lets effective reads use property-level packages.
            _unitPackageRepository.DeletePackagesWithItems(room.UnitPackages.ToList());
            room.UnitPackages.Clear();
            return;
        }

        // CUSTOM mode guarantees the default no-furniture option before applying requested custom packages.
        await EnsureDefaultPackageAsync(room, lookups, cancellationToken);
        foreach (var package in packages ?? [])
        {
            await SyncRoomPackageAsync(room, package, lookups, cancellationToken);
        }
    }

    /// <summary>
    /// Ensures a room override keeps the default no-furniture package.
    /// </summary>
    /// <param name="room">The tracked room graph.</param>
    /// <param name="lookups">The package lookup values required by synchronization.</param>
    /// <param name="cancellationToken">The token used to cancel the default-package mutation.</param>
    private async Task EnsureDefaultPackageAsync(
        Unit room,
        UnitPackageMutationLookupModel lookups,
        CancellationToken cancellationToken)
    {
        var defaultPackage = room.UnitPackages.FirstOrDefault(package =>
            string.Equals(
                package.PackageCode,
                MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE,
                StringComparison.OrdinalIgnoreCase));
        var defaultType = lookups.NoFurniturePackageType;

        if (defaultPackage is null)
        {
            // Room override sets keep the same default option as property-level package setup.
            defaultPackage = new UnitPackage
            {
                PublicId = Guid.NewGuid(),
                Property = room.Property,
                Unit = room,
                PackageCode = defaultType.Code
            };
            room.UnitPackages.Add(defaultPackage);
            await _unitPackageRepository.AddAsync(defaultPackage, cancellationToken);
        }

        defaultPackage.PackageTypeId = defaultType.Id;
        defaultPackage.PackageName = defaultType.Name;
        defaultPackage.Description = defaultType.Description;
        defaultPackage.PriceAdjustment = 0;
        defaultPackage.StatusId = lookups.ActiveStatusId;
    }

    /// <summary>
    /// Synchronizes one custom package template for the room.
    /// </summary>
    /// <param name="room">The tracked room graph.</param>
    /// <param name="packageTemplate">The edited package payload.</param>
    /// <param name="lookups">The package lookup values required by synchronization.</param>
    /// <param name="cancellationToken">The token used to cancel the package mutation.</param>
    private async Task SyncRoomPackageAsync(
        Unit room,
        UpdateRoomPackageRequestDto packageTemplate,
        UnitPackageMutationLookupModel lookups,
        CancellationToken cancellationToken)
    {
        var unitPackage = packageTemplate.Id.HasValue
            ? room.UnitPackages.FirstOrDefault(package => package.PublicId == packageTemplate.Id.Value)
            : null;

        if (packageTemplate.Id.HasValue && unitPackage is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.RoomErrors.ERROR_ROOM_PACKAGE_NOT_FOUND,
                BAD_REQUEST,
                StatusCodes.Status400BadRequest);
        }

        if (unitPackage is not null
            && string.Equals(
                unitPackage.PackageCode,
                MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE,
                StringComparison.OrdinalIgnoreCase))
        {
            // The default package is an invariant and cannot be edited through the custom package collection.
            throw new ApiException(
                ApplicationErrorConstants.RoomErrors.ERROR_ROOM_PACKAGE_NOT_FOUND,
                BAD_REQUEST,
                StatusCodes.Status400BadRequest);
        }

        var isNewPackage = unitPackage is null;

        if (isNewPackage)
        {
            // New room package overrides receive internal package codes managed only by the backend.
            unitPackage = new UnitPackage
            {
                PublicId = Guid.NewGuid(),
                Property = room.Property,
                Unit = room,
                PackageCode = CodeGenerationHelper.GenerateCode(
                    UNIT_PACKAGE_CODE_PREFIX,
                    UNIT_PACKAGE_CODE_RANDOM_LENGTH)
            };
            room.UnitPackages.Add(unitPackage);
            await _unitPackageRepository.AddAsync(unitPackage, cancellationToken);
        }

        // Mapster merges only submitted package fields; lookup-backed fields remain service-owned.
        packageTemplate.Adapt(unitPackage);

        if (isNewPackage)
        {
            // Internal type and status are creation defaults; partial updates preserve persisted values.
            unitPackage.PackageTypeId = lookups.CustomPackageTypeId;
            unitPackage.StatusId = lookups.ActiveStatusId;
        }

        await _unitPackageRepository.UpdateAsync(unitPackage);

        if (packageTemplate.Items is not null)
        {
            // Null keeps current items; an empty submitted collection intentionally clears them.
            await _unitPackageItemRepository.ReplaceItemsAsync(
                unitPackage,
                packageTemplate.Items.Select(item => item.Name).ToList(),
                cancellationToken);
        }
    }

    /// <summary>
    /// Builds the exact master-data keys needed by a room update.
    /// </summary>
    /// <param name="request">The room update request.</param>
    /// <returns>The master-data keys required by the update flow.</returns>
    private static IReadOnlyCollection<MasterDataKeyModel> GetUpdateMasterDataKeys(RoomUpdateRequestModel request)
    {
        // Keep one de-duplicated lookup set because the same master-data key can appear in several submitted rows.
        var keys = new HashSet<MasterDataKeyModel>();

        // COMMON mode never resolves room-level policy rows because it removes overrides and falls back to property setup.
        IReadOnlyList<UpdateRoomChargePolicyRequestDto> customChargePolicies =
            string.Equals(
                request.ChargePolicyMode,
                ROOM_OVERRIDE_MODE_CUSTOM,
                StringComparison.OrdinalIgnoreCase)
                ? request.ChargePolicies ?? []
                : [];

        // Scalar room selections only need a lookup when the edit payload supplies their code.
        if (request.TypeCode is not null)
        {
            keys.Add(new MasterDataKeyModel(MasterDataTypeEnum.UnitType, request.TypeCode));
        }

        if (request.RentalModeCode is not null)
        {
            keys.Add(new MasterDataKeyModel(MasterDataTypeEnum.UnitRentalMode, request.RentalModeCode));
        }

        // CUSTOM mode resolves only codes present in submitted new or edited policies.
        if (customChargePolicies.Any(policy => !policy.Id.HasValue))
        {
            keys.Add(new MasterDataKeyModel(MasterDataTypeEnum.CommonStatus, MASTER_CODE_ACTIVE));
        }

        // Each policy can select its charge line and, for parking, an optional vehicle type.
        foreach (var chargePolicy in customChargePolicies)
        {
            if (chargePolicy.Code is not null)
            {
                keys.Add(new MasterDataKeyModel(MasterDataTypeEnum.InvoiceLineType, chargePolicy.Code));
            }

            if (chargePolicy.VehicleTypeCode is not null)
            {
                keys.Add(new MasterDataKeyModel(MasterDataTypeEnum.VehicleType, chargePolicy.VehicleTypeCode));
            }
        }

        // CUSTOM package mode always needs default metadata, then adds the custom type only for new package rows.
        if (string.Equals(request.PackageMode, ROOM_OVERRIDE_MODE_CUSTOM, StringComparison.OrdinalIgnoreCase))
        {
            keys.Add(new MasterDataKeyModel(
                MasterDataTypeEnum.UnitPackageType,
                MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE));
            keys.Add(new MasterDataKeyModel(
                MasterDataTypeEnum.UnitPackageStatus,
                MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE));

            if ((request.Packages ?? []).Any(package => !package.Id.HasValue))
            {
                keys.Add(new MasterDataKeyModel(
                    MasterDataTypeEnum.UnitPackageType,
                    MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM));
            }
        }

        // The set is complete for the submitted partial update and naturally removes duplicate type/code pairs.
        return keys;
    }
}
