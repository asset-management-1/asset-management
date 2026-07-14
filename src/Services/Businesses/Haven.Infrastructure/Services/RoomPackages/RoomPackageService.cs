namespace Haven.Infrastructure.Services.RoomPackages;

/// <summary>
/// Provides standalone room package workflows while preserving common-package fallback semantics.
/// </summary>
public sealed class RoomPackageService : IRoomPackageService
{
    private readonly IUnitRepository _unitRepository;
    private readonly IUnitPackageRepository _unitPackageRepository;
    private readonly IUnitPackageItemRepository _unitPackageItemRepository;
    private readonly IMasterDataService _masterDataService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RoomPackageService> _logger;

    /// <summary>
    /// Creates the room package service.
    /// </summary>
    /// <param name="unitRepository">The repository used for scoped room reads and tracked package graphs.</param>
    /// <param name="unitPackageRepository">The repository used for package persistence and contract guards.</param>
    /// <param name="unitPackageItemRepository">The repository used to replace ordered package items.</param>
    /// <param name="masterDataService">The service used to resolve package types and statuses.</param>
    /// <param name="unitOfWork">The transaction boundary for package mutations.</param>
    /// <param name="logger">The structured room package workflow logger.</param>
    public RoomPackageService(
        IUnitRepository unitRepository,
        IUnitPackageRepository unitPackageRepository,
        IUnitPackageItemRepository unitPackageItemRepository,
        IMasterDataService masterDataService,
        IUnitOfWork unitOfWork,
        ILogger<RoomPackageService> logger)
    {
        _unitRepository = unitRepository;
        _unitPackageRepository = unitPackageRepository;
        _unitPackageItemRepository = unitPackageItemRepository;
        _masterDataService = masterDataService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Loads the effective package list for one landlord-scoped room.
    /// </summary>
    /// <param name="request">The room identifier and current landlord context.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The room header and user-managed effective package list.</returns>
    public async Task<RoomPackageListResponseDto> GetListAsync(
        RoomPackageListRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Build the exact landlord scope required by the room header and effective-package projections.
        var roomParameters = new RoomScopedQueryParametersModel
        {
            CurrentPartyId = request.CurrentParty.PartyId,
            RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES,
            RoomPublicId = request.RoomPublicId,
            NoFurniturePackageTypeCode = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE
        };
        var packageParameters = new RoomPackageQueryParametersModel
        {
            CurrentPartyId = request.CurrentParty.PartyId,
            RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES,
            RoomPublicId = request.RoomPublicId,
            ActiveStatusCode = MASTER_CODE_ACTIVE,
            NoFurniturePackageTypeCode = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE
        };

        // Load the scoped room header and user-managed packages independently under one landlord scope.
        var roomTask = _unitRepository.GetRoomDetailAsync(roomParameters, cancellationToken);
        var packageTask = _unitRepository.GetEffectivePackageTemplatesAsync(packageParameters, cancellationToken);

        await Task.WhenAll(roomTask, packageTask);

        var room = await roomTask
                   ?? throw new ApiException(
                       ApplicationErrorConstants.RoomErrors.ERROR_ROOM_NOT_FOUND,
                       NOT_FOUND,
                       StatusCodes.Status404NotFound);

        var response = RoomPackageResponseMapper.MapList(room, await packageTask);

        _logger.LogInformation(
            InfrastructureLogConstants.RoomPackageLogs.ROOM_PACKAGES_LOADED,
            response.Packages.Count,
            request.RoomPublicId,
            request.CurrentParty.PartyPublicId);

        return response;
    }

    /// <summary>
    /// Loads one package only when it belongs to the room's current effective package set.
    /// </summary>
    /// <param name="request">The room, package, and current landlord identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The requested user-managed package detail.</returns>
    public async Task<RoomPackageDetailResponseDto> GetDetailAsync(
        RoomPackageDetailRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Apply the same landlord relationship scope and hidden-fallback filter used by the package list.
        var roomParameters = new RoomScopedQueryParametersModel
        {
            CurrentPartyId = request.CurrentParty.PartyId,
            RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES,
            RoomPublicId = request.RoomPublicId,
            NoFurniturePackageTypeCode = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE
        };
        var packageParameters = new RoomPackageQueryParametersModel
        {
            CurrentPartyId = request.CurrentParty.PartyId,
            RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES,
            RoomPublicId = request.RoomPublicId,
            ActiveStatusCode = MASTER_CODE_ACTIVE,
            NoFurniturePackageTypeCode = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE
        };

        // Resolve the room and effective package set under the same landlord scope used by the list endpoint.
        var roomTask = _unitRepository.GetRoomDetailAsync(roomParameters, cancellationToken);
        var packageTask = _unitRepository.GetEffectivePackageTemplatesAsync(packageParameters, cancellationToken);

        await Task.WhenAll(roomTask, packageTask);

        var room = await roomTask
                   ?? throw new ApiException(
                       ApplicationErrorConstants.RoomErrors.ERROR_ROOM_NOT_FOUND,
                       NOT_FOUND,
                       StatusCodes.Status404NotFound);
        var list = RoomPackageResponseMapper.MapList(room, await packageTask);
        var package = list.Packages.FirstOrDefault(item => item.Id == request.PackagePublicId)
                      ?? throw new ApiException(
                          ApplicationErrorConstants.RoomPackageErrors.ERROR_ROOM_PACKAGE_NOT_FOUND,
                          NOT_FOUND,
                          StatusCodes.Status404NotFound);

        return RoomPackageResponseMapper.MapDetail(list, package);
    }

    /// <summary>
    /// Adds one room-specific package, materializing common packages when this is the first override.
    /// </summary>
    /// <param name="request">The submitted package fields and current landlord context.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The created room-owned package detail.</returns>
    public async Task<RoomPackageDetailResponseDto> CreateAsync(
        RoomPackageCreateRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Resolve the system package types and active status before entering the write transaction.
        var lookups = await ResolvePackageLookupsAsync(includeCustomType: true, cancellationToken);

        // Keep the resulting identifier outside the transaction so the committed projection can be reloaded afterward.
        var packagePublicId = await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
        {
            // Lock the landlord-scoped room and snapshot common packages before adding the new room-owned package.
            var room = await GetLockedRoomAsync(request.CurrentParty, request.RoomPublicId, transactionToken);
            var materialized = await MaterializeCommonPackagesAsync(room, lookups, transactionToken);

            // Enforce the room package limit after materialization so inherited packages count toward the final override set.
            if (room.UnitPackages.Count(package => !package.IsDeleted) >= MAX_PACKAGE_TEMPLATES)
            {
                throw new ApiException(
                    ApplicationErrorConstants.RoomPackageErrors.ERROR_ROOM_PACKAGE_LIMIT_EXCEEDED,
                    BAD_REQUEST);
            }

            // Map FE-owned display fields, then attach every backend-owned identity and relationship explicitly.
            var package = request.Adapt<UnitPackage>();
            package.PublicId = Guid.NewGuid();
            package.Property = room.Property;
            package.Unit = room;
            package.PackageCode = await GeneratePackageCodeAsync(room, materialized.UsedCodes, transactionToken);
            package.PackageTypeId = lookups.CustomPackageTypeId;
            package.StatusId = lookups.ActiveStatusId;

            // Stage the package and its ordered item rows within the same transaction.
            room.UnitPackages.Add(package);
            await _unitPackageRepository.AddAsync(package, transactionToken);
            await _unitPackageItemRepository.ReplaceItemsAsync(
                package,
                request.Items.Select(item => item.Name).ToList(),
                transactionToken);

            // Preserve the hidden zero-price fallback after the room becomes package-specific.
            await EnsureDefaultPackageAsync(room, lookups, transactionToken);

            _logger.LogInformation(
                InfrastructureLogConstants.RoomPackageLogs.ROOM_PACKAGE_CREATED,
                package.PublicId,
                room.PublicId,
                request.CurrentParty.PartyPublicId);

            return package.PublicId;
        }, cancellationToken);

        // Reload the committed projection so the response uses the same effective-package rules as normal reads.
        return await GetDetailAsync(
            new RoomPackageDetailRequestModel
            {
                CurrentParty = request.CurrentParty,
                RoomPublicId = request.RoomPublicId,
                PackagePublicId = packagePublicId
            },
            cancellationToken);
    }

    /// <summary>
    /// Applies submitted fields to one effective package without changing property-level common data.
    /// </summary>
    /// <param name="request">The package identity, partial fields, and current landlord context.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The updated room-owned package detail.</returns>
    public async Task<RoomPackageDetailResponseDto> UpdateAsync(
        RoomPackageUpdateRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Resolve only the fallback type and active status needed by copy-on-write mutation.
        var lookups = await ResolvePackageLookupsAsync(includeCustomType: false, cancellationToken);

        var packagePublicId = await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
        {
            // Lock the room, reject hidden fallback IDs, materialize inherited data, and return the editable room-owned target.
            var room = await GetLockedRoomAsync(request.CurrentParty, request.RoomPublicId, transactionToken);
            var package = await ResolveEditableRoomPackageAsync(
                room,
                request.PackagePublicId,
                lookups,
                transactionToken);

            // Apply only submitted scalar fields; omitted partial-update values retain their persisted state.
            request.Adapt(package);
            await _unitPackageRepository.UpdateAsync(package);

            if (request.Items is not null)
            {
                // Null preserves persisted items; an empty list clears them; a populated list replaces their order.
                await _unitPackageItemRepository.ReplaceItemsAsync(
                    package,
                    request.Items.Select(item => item.Name).ToList(),
                    transactionToken);
            }

            _logger.LogInformation(
                InfrastructureLogConstants.RoomPackageLogs.ROOM_PACKAGE_UPDATED,
                package.PublicId,
                room.PublicId,
                request.CurrentParty.PartyPublicId);

            return package.PublicId;
        }, cancellationToken);

        // Reload the committed projection so FE receives the room-owned ID created by copy-on-write when applicable.
        return await GetDetailAsync(
            new RoomPackageDetailRequestModel
            {
                CurrentParty = request.CurrentParty,
                RoomPublicId = request.RoomPublicId,
                PackagePublicId = packagePublicId
            },
            cancellationToken);
    }

    /// <summary>
    /// Soft-deletes one package when no contract references it and keeps the default package invariant.
    /// </summary>
    /// <param name="request">The package identity and current landlord context.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The successful delete operation status.</returns>
    public async Task<OperationStatusResponseDto> DeleteAsync(
        RoomPackageDeleteRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Resolve the fallback identity needed to keep system packages outside user-managed CRUD operations.
        var lookups = await ResolvePackageLookupsAsync(includeCustomType: false, cancellationToken);

        await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
        {
            // Lock the room and resolve the final room-owned target before any soft-delete state is staged.
            var room = await GetLockedRoomAsync(request.CurrentParty, request.RoomPublicId, transactionToken);
            var package = await ResolveEditableRoomPackageAsync(
                room,
                request.PackagePublicId,
                lookups,
                transactionToken);

            // Soft-delete the package and loaded items while preserving historical contract and billing references.
            _unitPackageRepository.DeletePackagesWithItems([package]);

            // Reassert the hidden fallback invariant when the deleted package was the room's last user-managed option.
            await EnsureDefaultPackageAsync(room, lookups, transactionToken);

            _logger.LogInformation(
                InfrastructureLogConstants.RoomPackageLogs.ROOM_PACKAGE_DELETED,
                package.PublicId,
                room.PublicId,
                request.CurrentParty.PartyPublicId);
        }, cancellationToken);

        return OperationStatusResponseHelper.Success(
            ApplicationMessageConstants.RoomPackageMessages.ROOM_PACKAGE_DELETE_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Loads and locks one room package graph inside the active transaction.
    /// </summary>
    /// <param name="currentParty">The current landlord context used to scope the room.</param>
    /// <param name="roomPublicId">The frontend-safe room identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the lock and graph load.</param>
    /// <returns>The tracked room package graph.</returns>
    private async Task<Unit> GetLockedRoomAsync(
        CurrentPartyContextModel currentParty,
        Guid roomPublicId,
        CancellationToken cancellationToken)
    {
        // Keep the mutation lock under the same active landlord relationship predicate as package reads.
        return await _unitRepository.GetRoomPackageGraphForMutationAsync(
                   new RoomScopedQueryParametersModel
                   {
                       CurrentPartyId = currentParty.PartyId,
                       RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES,
                       RoomPublicId = roomPublicId,
                       NoFurniturePackageTypeCode = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE
                   },
                   cancellationToken)
               ?? throw new ApiException(
                   ApplicationErrorConstants.RoomErrors.ERROR_ROOM_NOT_FOUND,
                   NOT_FOUND,
                   StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Copies the common package set into the room exactly once and returns source-to-copy identity mapping.
    /// </summary>
    /// <param name="room">The locked room receiving its package override set.</param>
    /// <param name="lookups">The resolved package types and active status.</param>
    /// <param name="cancellationToken">The token used to cancel persistence work.</param>
    /// <returns>The room package identities and generated-code set after materialization.</returns>
    private async Task<RoomPackageMaterializationModel> MaterializeCommonPackagesAsync(
        Unit room,
        UnitPackageMutationLookupModel lookups,
        CancellationToken cancellationToken)
    {
        var activeRoomPackages = room.UnitPackages.Where(package => !package.IsDeleted).ToList();
        var roomPackages = activeRoomPackages
            .Where(package => package.PackageTypeId != lookups.NoFurniturePackageType.Id)
            .ToList();
        var usedCodes = activeRoomPackages
            .Select(package => package.PackageCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (roomPackages.Count > 0)
        {
            return new RoomPackageMaterializationModel(
                roomPackages.ToDictionary(package => package.PublicId),
                usedCodes);
        }

        var sourceToRoomPackage = new Dictionary<Guid, UnitPackage>();
        var commonPackages = room.Property.UnitPackages
            .Where(package => package.UnitId is null && !package.IsDeleted)
            .ToList();
        var roomFallback = activeRoomPackages.FirstOrDefault(package =>
            package.PackageTypeId == lookups.NoFurniturePackageType.Id);

        // First mutation snapshots every common package into this room; later mutations see the room-owned set and skip cloning.
        foreach (var commonPackage in commonPackages)
        {
            var isDefault = commonPackage.PackageTypeId == lookups.NoFurniturePackageType.Id;

            // A room may already hold its internal fallback while still inheriting common user-managed packages.
            if (isDefault && roomFallback is not null)
            {
                sourceToRoomPackage[commonPackage.PublicId] = roomFallback;
                continue;
            }

            var packageCode = isDefault
                ? MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE
                : await GeneratePackageCodeAsync(room, usedCodes, cancellationToken);
            var roomPackage = new UnitPackage
            {
                PublicId = Guid.NewGuid(),
                Property = room.Property,
                Unit = room,
                PackageTypeId = commonPackage.PackageTypeId,
                PackageCode = packageCode,
                PackageName = commonPackage.PackageName,
                Description = commonPackage.Description,
                PriceAdjustment = commonPackage.PriceAdjustment,
                StatusId = commonPackage.StatusId
            };

            room.UnitPackages.Add(roomPackage);
            sourceToRoomPackage[commonPackage.PublicId] = roomPackage;
            usedCodes.Add(packageCode);

            await _unitPackageRepository.AddAsync(roomPackage, cancellationToken);
            await _unitPackageItemRepository.ReplaceItemsAsync(
                roomPackage,
                commonPackage.Items.OrderBy(item => item.DisplayOrder).Select(item => item.ItemName).ToList(),
                cancellationToken);
        }

        await EnsureDefaultPackageAsync(room, lookups, cancellationToken);

        return new RoomPackageMaterializationModel(sourceToRoomPackage, usedCodes);
    }

    /// <summary>
    /// Ensures each room-owned package set contains the immutable zero-price no-furniture option.
    /// </summary>
    /// <param name="room">The locked room whose package set must contain the fallback.</param>
    /// <param name="lookups">The resolved fallback package type and active status.</param>
    /// <param name="cancellationToken">The token used to cancel persistence work.</param>
    private async Task EnsureDefaultPackageAsync(
        Unit room,
        UnitPackageMutationLookupModel lookups,
        CancellationToken cancellationToken)
    {
        // Check the active room-owned graph first so repeated mutations preserve the existing system fallback.
        var defaultPackage = room.UnitPackages.FirstOrDefault(package =>
            !package.IsDeleted && package.PackageTypeId == lookups.NoFurniturePackageType.Id);

        // The invariant is already satisfied; avoid staging a duplicate fallback package.
        if (defaultPackage is not null)
        {
            return;
        }

        // Build the hidden zero-price package from canonical master data rather than FE-submitted package fields.
        var defaultType = lookups.NoFurniturePackageType;
        defaultPackage = new UnitPackage
        {
            PublicId = Guid.NewGuid(),
            Property = room.Property,
            Unit = room,
            PackageTypeId = defaultType.Id,
            PackageCode = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE,
            PackageName = defaultType.Name,
            Description = defaultType.Description,
            PriceAdjustment = 0,
            StatusId = lookups.ActiveStatusId
        };

        // Attach and persist the fallback in the active transaction so the room always retains a valid no-package option.
        room.UnitPackages.Add(defaultPackage);
        await _unitPackageRepository.AddAsync(defaultPackage, cancellationToken);
    }

    /// <summary>
    /// Resolves the editable room-owned target for one effective user-managed package.
    /// </summary>
    /// <param name="room">The tracked room containing common and room-specific packages.</param>
    /// <param name="packagePublicId">The requested package identifier.</param>
    /// <param name="lookups">The resolved fallback type and active package status.</param>
    /// <param name="cancellationToken">The token used to cancel materialization and dependency checks.</param>
    /// <returns>The editable room-owned package after copy-on-write materialization.</returns>
    private async Task<UnitPackage> ResolveEditableRoomPackageAsync(
        Unit room,
        Guid packagePublicId,
        UnitPackageMutationLookupModel lookups,
        CancellationToken cancellationToken)
    {
        var noFurniturePackageTypeId = lookups.NoFurniturePackageType.Id;
        var roomPackages = room.UnitPackages
            .Where(package => !package.IsDeleted && package.PackageTypeId != noFurniturePackageTypeId)
            .ToList();
        var effectivePackages = roomPackages.Count > 0
            ? roomPackages
            : room.Property.UnitPackages.Where(package => package.UnitId is null && !package.IsDeleted).ToList();

        // Validate against the pre-materialization effective set so fallback IDs cannot create room overrides as a side effect.
        var sourcePackage = effectivePackages.FirstOrDefault(package =>
                                package.PublicId == packagePublicId
                                && package.PackageTypeId != noFurniturePackageTypeId)
                            ?? throw new ApiException(
                                ApplicationErrorConstants.RoomPackageErrors.ERROR_ROOM_PACKAGE_NOT_FOUND,
                                NOT_FOUND,
                                StatusCodes.Status404NotFound);

        // Materialize inherited common packages once, then translate the submitted source ID to its room-owned copy.
        var materialized = await MaterializeCommonPackagesAsync(room, lookups, cancellationToken);
        var targetPackage = materialized.SourceToRoomPackage[sourcePackage.PublicId];

        // Dependency checks must target the room-owned package so contracts in other rooms never block this mutation.
        if (await _unitPackageRepository.HasContractReferenceAsync(targetPackage.Id, cancellationToken))
        {
            throw new ApiException(
                ApplicationErrorConstants.RoomPackageErrors.ERROR_ROOM_PACKAGE_MUTATION_BLOCKED,
                BAD_REQUEST);
        }

        return targetPackage;
    }

    /// <summary>
    /// Generates one backend-owned package code that is unique inside the room.
    /// </summary>
    /// <param name="room">The room receiving the generated package code.</param>
    /// <param name="usedCodes">The package codes already reserved in the in-memory room graph.</param>
    /// <param name="cancellationToken">The token used to cancel uniqueness checks.</param>
    /// <returns>The generated room-unique package code.</returns>
    private async Task<string> GeneratePackageCodeAsync(
        Unit room,
        ISet<string> usedCodes,
        CancellationToken cancellationToken)
    {
        var code = await CodeGenerationHelper.GenerateUniqueCodeAsync(
            new UniqueCodeGenerationOptions
            {
                Prefix = UNIT_PACKAGE_CODE_PREFIX,
                RandomLength = UNIT_PACKAGE_CODE_RANDOM_LENGTH,
                MaxAttempts = UNIT_PACKAGE_CODE_MAX_ATTEMPTS,
                ExistsAsync = async (candidate, token) =>
                    usedCodes.Contains(candidate)
                    || await _unitPackageRepository.ExistsPackageCodeAsync(room.Id, candidate, token),
                ExhaustionExceptionFactory = () => new ApiException(
                    ApplicationErrorConstants.RoomPackageErrors.ERROR_ROOM_PACKAGE_CODE_GENERATION_FAILED,
                    BAD_REQUEST)
            },
            cancellationToken);

        usedCodes.Add(code);
        return code;
    }

    /// <summary>
    /// Resolves the exact package type and status values required by one mutation.
    /// </summary>
    /// <param name="includeCustomType">Whether the mutation creates a custom package.</param>
    /// <param name="cancellationToken">The token used to cancel the master-data lookup.</param>
    /// <returns>The typed package mutation lookups.</returns>
    private async Task<UnitPackageMutationLookupModel> ResolvePackageLookupsAsync(
        bool includeCustomType,
        CancellationToken cancellationToken)
    {
        var keys = new List<MasterDataKeyModel>
        {
            new(MasterDataTypeEnum.UnitPackageType, MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE),
            new(MasterDataTypeEnum.UnitPackageStatus, MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE)
        };

        if (includeCustomType)
        {
            keys.Add(new MasterDataKeyModel(
                MasterDataTypeEnum.UnitPackageType,
                MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM));
        }

        var values = await _masterDataService.GetValuesAsync(keys, cancellationToken);
        return MasterDataLookupMapper.BuildUnitPackageLookups(values, includeCustomType);
    }

}
