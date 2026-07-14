namespace Haven.Infrastructure.Services.Properties;

/// <summary>
/// Provides property list, detail, and creation workflows for landlord-managed buildings.
/// </summary>
public class PropertyService : IPropertyService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IUnitPackageRepository _unitPackageRepository;
    private readonly IUnitPackageItemRepository _unitPackageItemRepository;
    private readonly IPropertyPartyRepository _propertyPartyRepository;
    private readonly IRentalChargePolicyRepository _rentalChargePolicyRepository;
    private readonly IMasterDataService _masterDataService;
    private readonly ILocationService _locationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PropertyService> _logger;

    /// <summary>
    /// Creates the property service.
    /// </summary>
    /// <param name="repositories">The grouped property repositories.</param>
    /// <param name="masterDataService">The service that resolves master-data values.</param>
    /// <param name="locationService">The service that resolves location values.</param>
    /// <param name="unitOfWork">The unit of work used for transactional writes.</param>
    /// <param name="logger">The service logger.</param>
    public PropertyService(
        PropertyRepositoryDependencies repositories,
        IMasterDataService masterDataService,
        ILocationService locationService,
        IUnitOfWork unitOfWork,
        ILogger<PropertyService> logger)
    {
        _propertyRepository = repositories.PropertyRepository;
        _unitRepository = repositories.UnitRepository;
        _unitPackageRepository = repositories.UnitPackageRepository;
        _unitPackageItemRepository = repositories.UnitPackageItemRepository;
        _propertyPartyRepository = repositories.PropertyPartyRepository;
        _rentalChargePolicyRepository = repositories.RentalChargePolicyRepository;
        _masterDataService = masterDataService;
        _locationService = locationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Loads paged properties scoped to the current party.
    /// </summary>
    /// <param name="request">The party-scoped list request.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The paged property response.</returns>
    public async Task<PaginationResponse<IReadOnlyList<PropertyListItemResponseDto>>> GetPropertiesAsync(
        PropertyListRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Handler resolves party context before this point; repository reads use only that party scope.
        var parameters = request.Adapt<PropertyListQueryParametersModel>();

        // Load page rows once; each row carries the window-count total for the filtered result set.
        var propertyRows = await _propertyRepository.GetPropertyPageAsync(parameters, cancellationToken);
        var total = propertyRows.FirstOrDefault()?.TotalCount ?? 0;

        var childParameters = request.Adapt<PropertyChildRowsQueryParametersModel>();
        childParameters.PropertyPublicIds = propertyRows.Select(x => x.PropertyPublicId).ToArray();

        // Child reads run in parallel; repositories own empty-id guards for blank result pages.
        var floorRowsTask = _propertyRepository.GetFloorsAsync(childParameters, cancellationToken);
        var roomRowsTask = _propertyRepository.GetRoomsAsync(childParameters, cancellationToken);
        var roomTenantRowsTask = _propertyRepository.GetRoomTenantsAsync(childParameters, cancellationToken);

        await Task.WhenAll(floorRowsTask, roomRowsTask, roomTenantRowsTask);

        var floorRows = await floorRowsTask;
        var roomRows = await roomRowsTask;
        var roomTenantRows = await roomTenantRowsTask;

        var response = new PaginationResponse<IReadOnlyList<PropertyListItemResponseDto>>(
            PropertyResponseMapper.MapList(propertyRows, floorRows, roomRows, roomTenantRows),
            request.PageNumber,
            request.PageSize,
            total);

        _logger.LogInformation(
            InfrastructureLogConstants.PropertyLogs.PROPERTIES_LOADED,
            propertyRows.Count,
            request.CurrentParty.PartyPublicId);

        return response;
    }

    /// <summary>
    /// Loads one property detail scoped to the current party.
    /// </summary>
    /// <param name="request">The party-scoped detail request.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The property detail response.</returns>
    public async Task<PropertyDetailResponseDto> GetPropertyDetailAsync(
        PropertyDetailRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Handler resolves party context before this point; detail lookup hides cross-party properties.
        var parameters = request.Adapt<PropertyScopedQueryParametersModel>();

        var propertyRow = await _propertyRepository.GetPropertyDetailHeaderAsync(parameters, cancellationToken);

        // Treat missing or unauthorized properties the same from the caller's perspective.
        if (propertyRow is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_NOT_FOUND,
                NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        // Detail uses derived floor/room shape plus editable policy/package sections.
        var childParameters = new PropertyChildRowsQueryParametersModel
        {
            PropertyPublicIds = [propertyRow.PropertyPublicId]
        };
        var floorRowsTask = _propertyRepository.GetFloorsAsync(childParameters, cancellationToken);
        var roomRowsTask = _propertyRepository.GetRoomsAsync(childParameters, cancellationToken);
        var chargePoliciesTask = _propertyRepository.GetChargePoliciesAsync(
            request.PropertyPublicId,
            cancellationToken);
        var packageTemplatesTask = _propertyRepository.GetPackageTemplatesAsync(
            request.PropertyPublicId,
            cancellationToken);
        var wholeBuildingRentalTask = _propertyRepository.GetWholeBuildingRentalAsync(
            request.PropertyPublicId,
            cancellationToken);

        await Task.WhenAll(
            floorRowsTask,
            roomRowsTask,
            chargePoliciesTask,
            packageTemplatesTask,
            wholeBuildingRentalTask);

        var floorRows = await floorRowsTask;
        var roomRows = await roomRowsTask;
        var chargePolicies = await chargePoliciesTask;
        var packageTemplates = await packageTemplatesTask;
        var wholeBuildingRental = await wholeBuildingRentalTask;

        _logger.LogInformation(
            InfrastructureLogConstants.PropertyLogs.PROPERTY_DETAIL_LOADED,
            request.PropertyPublicId,
            request.CurrentParty.PartyPublicId);

        return PropertyResponseMapper.MapDetail(
            propertyRow,
            floorRows,
            roomRows,
            chargePolicies,
            packageTemplates,
            wholeBuildingRental);
    }

    /// <summary>
    /// Creates a property from the final frontend structure, common packages, and optional charge policies.
    /// </summary>
    /// <param name="request">The party-scoped property creation request.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The created property summary.</returns>
    public async Task<CreatedPropertyResponseDto> CreatePropertyAsync(
        PropertyCreationRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Application has shaped the request; service validates only the master-data values needed for mapping.
        var masterDataKeys = GetCreateMasterDataKeys(request);
        var masterData = await _masterDataService.GetValuesAsync(masterDataKeys, cancellationToken);

        // Convert the resolved master-data rows into the typed IDs and values consumed by graph mapping.
        var lookups = BuildCreateLookups(request, masterData);

        // Location codes are optional, but supplied province/district/ward codes must resolve consistently.
        request.Locations = await _locationService.GetLocationsAsync(
            request.Adapt<LocationKeyModel>(),
            cancellationToken);

        // Property codes are generated before mapping so the persisted graph and response share one value.
        request.PropertyCode = await CodeGenerationHelper.GenerateUniqueCodeAsync(
            new UniqueCodeGenerationOptions
            {
                Prefix = PROPERTY_CODE_PREFIX,
                RandomLength = PROPERTY_CODE_RANDOM_LENGTH,
                MaxAttempts = PROPERTY_CODE_MAX_ATTEMPTS,
                ExistsAsync = _propertyRepository.PropertyCodeExistsAsync,
                ExhaustionExceptionFactory = () => new ApiException(
                    ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_CODE_GENERATION_FAILED,
                    INTERNAL_SERVER,
                    StatusCodes.Status500InternalServerError)
            },
            cancellationToken);

        // Persist the property graph atomically so partial property/unit setup cannot leak to readers.
        return await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
        {
            // EF is used here because create must persist one mapped property graph atomically in a transaction.
            var graph = new PropertyCreationGraphInputModel
            {
                Request = request,
                Lookups = lookups
            }.Adapt<PropertyCreationGraphModel>();

            // Persist the aggregate root and dependent room/package graph in parent-to-child order.
            await _propertyRepository.AddAsync(graph.Property, transactionToken);
            await _unitRepository.AddRangeAsync(graph.Units, transactionToken);
            await _unitPackageRepository.AddRangeAsync(graph.UnitPackages, transactionToken);
            await _unitPackageItemRepository.AddRangeAsync(graph.UnitPackageItems, transactionToken);

            // Persist landlord access and common charge policies after their property root exists.
            await _propertyPartyRepository.AddAsync(graph.PropertyParty, transactionToken);
            await _rentalChargePolicyRepository.AddRangeAsync(graph.ChargePolicies, transactionToken);

            // Build the creation response from the same in-memory graph that was committed.
            var response = PropertyResponseMapper.MapCreated(graph);

            _logger.LogInformation(
                InfrastructureLogConstants.PropertyLogs.PROPERTY_CREATED,
                graph.Property.PublicId,
                graph.Units.Count,
                request.CurrentParty.PartyPublicId);

            return response;
        }, cancellationToken);
    }

    /// <summary>
    /// Updates a property edit form and returns refreshed property detail.
    /// </summary>
    /// <param name="request">The party-scoped update request.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The refreshed property detail response.</returns>
    public async Task<PropertyDetailResponseDto> UpdatePropertyAsync(
        PropertyUpdateRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Access check uses the same Dapper detail scope as GET detail so unauthorized properties look not found.
        await EnsurePropertyInCurrentPartyScopeAsync(
            request.PropertyPublicId,
            request.CurrentParty,
            cancellationToken);

        // Resolve only combobox codes submitted in this update payload.
        var masterData = await _masterDataService.GetValuesAsync(
            GetUpdateMasterDataKeys(request),
            cancellationToken);
        request.PropertyTypeId = request.PropertyTypeCode is null
            ? null
            : masterData.GetValue(MasterDataTypeEnum.PropertyType, request.PropertyTypeCode).Id;

        // Resolve the hierarchical location only when the partial edit submits at least one address selection code.
        var hasLocationEdit = !string.IsNullOrWhiteSpace(request.ProvinceCode)
                              || !string.IsNullOrWhiteSpace(request.DistrictCode)
                              || !string.IsNullOrWhiteSpace(request.WardCode);
        if (hasLocationEdit)
        {
            request.Locations = await _locationService.GetLocationsAsync(
                request.Adapt<LocationKeyModel>(),
                cancellationToken);
            request.Locations.Adapt(request);
        }

        // Load the tracked aggregate after access and lookup validation so the transaction can stay focused.
        var property = await GetTrackedPropertyGraphAsync(request.PropertyPublicId, cancellationToken);

        // FE sends only room rows it wants to add or update; omitted rooms stay unchanged.
        var updateRooms = (request.Structure?.Floors ?? [])
            .SelectMany(floor => (floor.Rooms ?? []).Select(room => new PropertyUpdateRoomModel
            {
                FloorNumber = floor.FloorNumber,
                Room = room
            }))
            .ToList();

        // Resolve room mutation fields and lookup IDs once so guard and persistence phases share one prepared view.
        foreach (var updateRoom in updateRooms)
        {
            updateRoom.Fields = updateRoom.Adapt<RoomFieldUpdateModel>();
            updateRoom.Lookups = MasterDataLookupMapper.BuildRoomFieldLookups(
                updateRoom.Fields,
                masterData,
                includeAvailableStatus: !updateRoom.Room.Id.HasValue);
        }

        // Package code is internal; update payloads identify existing common templates by public id only.
        var packageTemplates = (request.Packages ?? [])
            .Select(package => new PropertyUpdatePackageModel
            {
                Package = package
            })
            .ToList();
        var chargePolicyMutations = (request.ChargePolicies ?? [])
            .Select(policy => new PropertyChargePolicyMutationModel
            {
                Policy = policy,
                Lookups = MasterDataLookupMapper.BuildChargePolicyLookups(
                    policy,
                    masterData,
                    includeActiveStatus: !policy.Id.HasValue)
            })
            .ToList();
        var hasNewPackage = request.Packages?.Any(package => !package.Id.HasValue) == true;
        var packageLookups = request.Packages is null
            ? null
            : MasterDataLookupMapper.BuildUnitPackageLookups(masterData, hasNewPackage);

        // Run every dependency and protected-field guard before the transaction changes any tracked row.
        await ValidateUpdateGuardsAsync(
            property,
            request,
            updateRooms,
            packageTemplates,
            cancellationToken);

        // Persist only submitted sections after all conflicts are known.
        await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
        {
            // Mapster merges submitted scalar fields and the resolved persistence identifiers in one pass.
            request.Adapt(property);

            if (request.Structure is not null)
            {
                // Synchronize only the rooms explicitly submitted by the edit form; omitted rooms remain unchanged.
                await SyncUnitsAsync(property, updateRooms, transactionToken);

                // Refresh persisted structure counters only when submitted room rows changed
                // the tracked unit collection.
                property.RecomputeStructureTotals();
            }

            if (request.ChargePolicies is not null)
            {
                // Apply only submitted common policies without touching room-level override policies.
                await SyncChargePoliciesAsync(property, chargePolicyMutations, transactionToken);
            }

            if (request.Packages is not null)
            {
                // Apply only submitted common packages; rooms with custom package overrides keep their own setup.
                await SyncPackagesAsync(property, packageTemplates, packageLookups, transactionToken);
            }

            await _propertyRepository.UpdateAsync(property);

            _logger.LogInformation(
                InfrastructureLogConstants.PropertyLogs.PROPERTY_UPDATED,
                request.PropertyPublicId,
                request.CurrentParty.PartyPublicId);
        }, cancellationToken);

        // Return the same detail shape FE uses to refresh the edit/detail screen.
        return await GetPropertyDetailAsync(
            new PropertyDetailRequestModel
            {
                CurrentParty = request.CurrentParty,
                PropertyPublicId = request.PropertyPublicId
            },
            cancellationToken);
    }

    /// <summary>
    /// Schedules a property delete when no contract or active occupancy blocks it.
    /// </summary>
    /// <param name="request">The party-scoped delete request.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The operation status response.</returns>
    public async Task<OperationStatusResponseDto> DeletePropertyAsync(
        PropertyDeleteRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Access check happens before mutation guards so missing and unauthorized properties both return 404.
        await EnsurePropertyInCurrentPartyScopeAsync(
            request.PropertyPublicId,
            request.CurrentParty,
            cancellationToken);

        var property = await GetTrackedPropertyGraphAsync(request.PropertyPublicId, cancellationToken);
        var guard = await _propertyRepository.GetPropertyMutationGuardAsync(
            request.PropertyPublicId,
            cancellationToken);
        if (guard.HasDeleteBlockingDependencies)
        {
            throw new ApiException(ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_DELETE_BLOCKED, BAD_REQUEST);
        }

        // Delete is recoverable for seven days; final cleanup belongs to a later scheduled cleanup flow.
        await _unitOfWork.ExecuteInTransactionAsync(async _ =>
        {
            var requestedAt = DateTime.UtcNow;
            property.DeleteRequestedAt = requestedAt;
            property.DeleteScheduledAt = requestedAt.AddDays(PROPERTY_PENDING_DELETE_DAYS);
            property.DeleteRequestedByPartyId = request.CurrentParty.PartyId;
            await _propertyRepository.UpdateAsync(property);

            _logger.LogInformation(
                InfrastructureLogConstants.PropertyLogs.PROPERTY_DELETED,
                request.PropertyPublicId,
                property.DeleteScheduledAt,
                request.CurrentParty.PartyPublicId);
        }, cancellationToken);

        return OperationStatusResponseHelper.Success(
            ApplicationMessageConstants.PropertyMessages.PROPERTY_DELETE_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Restores a pending property delete request.
    /// </summary>
    /// <param name="request">The party-scoped restore-delete request.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The operation status response.</returns>
    public async Task<OperationStatusResponseDto> RestorePropertyDeleteAsync(
        PropertyDeleteRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Access check happens before mutation so missing and unauthorized properties both return 404.
        await EnsurePropertyInCurrentPartyScopeAsync(
            request.PropertyPublicId,
            request.CurrentParty,
            cancellationToken);

        var property = await GetTrackedPropertyGraphAsync(request.PropertyPublicId, cancellationToken);

        await _unitOfWork.ExecuteInTransactionAsync(async _ =>
        {
            property.DeleteRequestedAt = null;
            property.DeleteScheduledAt = null;
            property.DeleteRequestedByPartyId = null;
            await _propertyRepository.UpdateAsync(property);

            _logger.LogInformation(
                InfrastructureLogConstants.PropertyLogs.PROPERTY_DELETE_RESTORED,
                request.PropertyPublicId,
                request.CurrentParty.PartyPublicId);
        }, cancellationToken);

        return OperationStatusResponseHelper.Success(
            ApplicationMessageConstants.PropertyMessages.PROPERTY_RESTORE_DELETE_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Ensures the requested property is visible under the current party scope.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="currentParty">The current landlord party context.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    private async Task EnsurePropertyInCurrentPartyScopeAsync(
        Guid propertyPublicId,
        CurrentPartyContextModel currentParty,
        CancellationToken cancellationToken)
    {
        // Query the scoped header only, because this phase checks ownership rather than loading a write graph.
        var row = await _propertyRepository.GetPropertyDetailHeaderAsync(
            new PropertyScopedQueryParametersModel
            {
                CurrentPartyId = currentParty.PartyId,
                PropertyPublicId = propertyPublicId,
                RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES
            },
            cancellationToken);

        if (row is null)
        {
            // Treat unavailable and out-of-scope properties the same to avoid disclosing another landlord's data.
            throw new ApiException(
                ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_NOT_FOUND,
                NOT_FOUND,
                StatusCodes.Status404NotFound);
        }
    }

    /// <summary>
    /// Loads the tracked property graph needed for write synchronization.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tracked property graph.</returns>
    private async Task<Property> GetTrackedPropertyGraphAsync(
        Guid propertyPublicId,
        CancellationToken cancellationToken)
    {
        // Load the full tracked graph only after scope validation because subsequent sync phases mutate child collections.
        var property = await _propertyRepository.GetPropertyGraphByPublicIdAsync(
            propertyPublicId,
            cancellationToken);

        return property ?? throw new ApiException(
            ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_NOT_FOUND,
            NOT_FOUND,
            StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Checks submitted update operations before EF mutation starts.
    /// </summary>
    /// <param name="property">The tracked property graph.</param>
    /// <param name="request">The validated update request.</param>
    /// <param name="updateRooms">The flattened room edit rows.</param>
    /// <param name="packageTemplates">The edited package templates.</param>
    /// <param name="cancellationToken">The token used to cancel guard reads.</param>
    private async Task ValidateUpdateGuardsAsync(
        Property property,
        PropertyUpdateRequestModel request,
        IReadOnlyList<PropertyUpdateRoomModel> updateRooms,
        IReadOnlyList<PropertyUpdatePackageModel> packageTemplates,
        CancellationToken cancellationToken)
    {
        // Each nullable section is guarded only when submitted; omitted sections preserve the stored graph.
        if (request.Structure is not null)
        {
            await ValidateStructureUpdateGuardsAsync(
                property,
                request,
                updateRooms,
                cancellationToken);
        }

        if (request.ChargePolicies is not null)
        {
            ValidateSubmittedChargePolicyIds(property, request);
        }

        if (request.Packages is not null)
        {
            ValidateSubmittedPackageIds(property, request, packageTemplates);
        }
    }

    /// <summary>
    /// Validates submitted room edits before the tracked room graph is mutated.
    /// </summary>
    /// <param name="property">The tracked property graph.</param>
    /// <param name="request">The validated update request.</param>
    /// <param name="updateRooms">The flattened room edit rows.</param>
    /// <param name="cancellationToken">The token used to cancel guard reads.</param>
    private async Task ValidateStructureUpdateGuardsAsync(
        Property property,
        PropertyUpdateRequestModel request,
        IReadOnlyList<PropertyUpdateRoomModel> updateRooms,
        CancellationToken cancellationToken)
    {
        var existingRoomsById = property.Units.ToDictionary(unit => unit.PublicId);

        // Existing room ids in the payload must belong to this property.
        if (updateRooms.Any(updateRoom =>
                updateRoom.Room.Id.HasValue
                && !existingRoomsById.ContainsKey(updateRoom.Room.Id.Value)))
        {
            // Keep response stable for FE while logging the internal preflight reason.
            _logger.LogWarning(
                InfrastructureLogConstants.PropertyLogs.PROPERTY_UPDATE_PREFLIGHT_REJECTED,
                request.PropertyPublicId,
                request.CurrentParty.PartyPublicId,
                ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_ROOM_NOT_FOUND);

            throw new ApiException(
                ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_UPDATE_PREFLIGHT_FAILED,
                ApplicationErrorConstants.PropertyErrors.PROPERTY_UPDATE_CONFLICT);
        }

        var requestedExistingRoomIds = updateRooms
            .Where(updateRoom => updateRoom.Room.Id.HasValue)
            .Select(updateRoom => updateRoom.Room.Id.Value)
            .Distinct()
            .ToArray();
        var unitGuards = (await _propertyRepository.GetUnitMutationGuardsAsync(
                requestedExistingRoomIds,
                cancellationToken))
            .ToDictionary(guard => guard.UnitPublicId);

        // Rooms with contracts or active occupancy can keep safe display edits only.
        if (HasProtectedRoomChanges(existingRoomsById, updateRooms, unitGuards))
        {
            // Protected room mutations are rejected before any EF changes are staged.
            _logger.LogWarning(
                InfrastructureLogConstants.PropertyLogs.PROPERTY_UPDATE_PREFLIGHT_REJECTED,
                request.PropertyPublicId,
                request.CurrentParty.PartyPublicId,
                ApplicationErrorConstants.RoomErrors.ERROR_ROOM_MUTATION_BLOCKED);

            throw new ApiException(
                ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_UPDATE_PREFLIGHT_FAILED,
                ApplicationErrorConstants.PropertyErrors.PROPERTY_UPDATE_CONFLICT);
        }
    }

    /// <summary>
    /// Checks whether protected fields are changed on rooms with active dependencies.
    /// </summary>
    /// <param name="existingRoomsById">The current rooms keyed by public id.</param>
    /// <param name="updateRooms">The flattened room edit rows.</param>
    /// <param name="unitGuards">The dependency guard counters keyed by room public id.</param>
    /// <returns><c>true</c> when a protected field edit must be blocked.</returns>
    private static bool HasProtectedRoomChanges(
        IReadOnlyDictionary<Guid, Unit> existingRoomsById,
        IReadOnlyList<PropertyUpdateRoomModel> updateRooms,
        IReadOnlyDictionary<Guid, UnitMutationGuardModel> unitGuards)
    {
        return updateRooms.Any(updateRoom =>
        {
            var roomId = updateRoom.Room.Id;

            // New rooms have no persisted dependencies, while unknown existing ids were rejected earlier.
            if (!roomId.HasValue || !existingRoomsById.TryGetValue(roomId.Value, out var existingRoom))
            {
                return false;
            }

            var guard = unitGuards.GetValueOrDefault(existingRoom.PublicId);

            return guard?.HasDeleteBlockingDependencies == true
                   && RoomFieldMutationMapper.HasProtectedFieldChanges(
                       existingRoom,
                       updateRoom.Fields,
                       updateRoom.Lookups);
        });
    }

    /// <summary>
    /// Validates submitted common charge-policy ids.
    /// </summary>
    /// <param name="property">The tracked property graph.</param>
    /// <param name="request">The validated update request.</param>
    private void ValidateSubmittedChargePolicyIds(
        Property property,
        PropertyUpdateRequestModel request)
    {
        var commonPolicyIds = property.RentalChargePolicies
            .Where(policy => policy.UnitId is null)
            .Select(policy => policy.PublicId)
            .ToHashSet();

        // Existing policy ids must point to common property-level policies, not room overrides.
        if (request.ChargePolicies.Any(policy =>
                policy.Id.HasValue && !commonPolicyIds.Contains(policy.Id.Value)))
        {
            // Policy ids from another scope are logged internally and returned as one safe preflight error.
            _logger.LogWarning(
                InfrastructureLogConstants.PropertyLogs.PROPERTY_UPDATE_PREFLIGHT_REJECTED,
                request.PropertyPublicId,
                request.CurrentParty.PartyPublicId,
                ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_CHARGE_POLICY_NOT_FOUND);

            throw new ApiException(
                ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_UPDATE_PREFLIGHT_FAILED,
                ApplicationErrorConstants.PropertyErrors.PROPERTY_UPDATE_CONFLICT);
        }
    }

    /// <summary>
    /// Validates submitted common package ids.
    /// </summary>
    /// <param name="property">The tracked property graph.</param>
    /// <param name="request">The validated update request.</param>
    /// <param name="packageTemplates">The edited package templates.</param>
    private void ValidateSubmittedPackageIds(
        Property property,
        PropertyUpdateRequestModel request,
        IReadOnlyList<PropertyUpdatePackageModel> packageTemplates)
    {
        var commonPackageIds = property.UnitPackages
            .Where(unitPackage => unitPackage.UnitId is null
                                  && !string.Equals(
                                      unitPackage.PackageCode,
                                      MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE,
                                      StringComparison.OrdinalIgnoreCase))
            .Select(unitPackage => unitPackage.PublicId)
            .ToHashSet();

        // Existing package ids must point to property-level common package templates.
        if (packageTemplates.Any(package =>
                package.Package.Id.HasValue && !commonPackageIds.Contains(package.Package.Id.Value)))
        {
            // Package ids from another scope are logged internally and returned as one safe preflight error.
            _logger.LogWarning(
                InfrastructureLogConstants.PropertyLogs.PROPERTY_UPDATE_PREFLIGHT_REJECTED,
                request.PropertyPublicId,
                request.CurrentParty.PartyPublicId,
                ApplicationErrorConstants.RoomErrors.ERROR_ROOM_PACKAGE_NOT_FOUND);

            throw new ApiException(
                ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_UPDATE_PREFLIGHT_FAILED,
                ApplicationErrorConstants.PropertyErrors.PROPERTY_UPDATE_CONFLICT);
        }
    }

    /// <summary>
    /// Synchronizes the edited room set into the tracked property graph.
    /// </summary>
    /// <param name="property">The tracked property graph.</param>
    /// <param name="updateRooms">The flattened room edit rows.</param>
    /// <param name="cancellationToken">The token used to cancel the staged mutation.</param>
    private async Task SyncUnitsAsync(
        Property property,
        IReadOnlyList<PropertyUpdateRoomModel> updateRooms,
        CancellationToken cancellationToken)
    {
        var existingById = property.Units.ToDictionary(unit => unit.PublicId);

        foreach (var updateRoom in updateRooms)
        {
            if (updateRoom.Room.Id.HasValue)
            {
                if (!existingById.TryGetValue(updateRoom.Room.Id.Value, out var unit))
                {
                    throw new ApiException(
                        ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_ROOM_NOT_FOUND,
                        NOT_FOUND,
                        StatusCodes.Status404NotFound);
                }

                // Existing rooms are updated only for fields submitted by FE.
                RoomFieldMutationMapper.Apply(unit, updateRoom.Fields, updateRoom.Lookups);
                await _unitRepository.UpdateAsync(unit);
                continue;
            }

            // New submitted rooms receive a backend-owned business code before they join the tracked graph.
            var unitCode = await GenerateUnitCodeAsync(property, cancellationToken);
            var newUnit = CreateUnit(property, unitCode, updateRoom.Fields, updateRoom.Lookups);
            property.Units.Add(newUnit);
            await _unitRepository.AddAsync(newUnit, cancellationToken);
        }
    }

    /// <summary>
    /// Generates a property-scoped room/unit business code for a new room.
    /// </summary>
    /// <param name="property">The tracked parent property.</param>
    /// <param name="cancellationToken">The token used to cancel uniqueness checks.</param>
    /// <returns>The generated room/unit business code.</returns>
    private async Task<string> GenerateUnitCodeAsync(Property property, CancellationToken cancellationToken)
    {
        var occupiedUnitCodes = property.Units
            .Select(unit => unit.UnitCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Update-property can add rooms to an existing property, so check both the tracked graph and the database.
        for (var attempt = 0; attempt < ROOM_CODE_MAX_ATTEMPTS; attempt++)
        {
            var candidate = CodeGenerationHelper.GenerateCode(ROOM_CODE_PREFIX, ROOM_CODE_RANDOM_LENGTH);

            if (!occupiedUnitCodes.Add(candidate))
            {
                continue;
            }

            if (!await _unitRepository.ExistsUnitCodeAsync(property.Id, candidate, cancellationToken))
            {
                return candidate;
            }
        }

        throw new ApiException(ApplicationErrorConstants.RoomErrors.ERROR_ROOM_CODE_GENERATION_FAILED, BAD_REQUEST);
    }

    /// <summary>
    /// Creates a new unit from an edited room row.
    /// </summary>
    /// <param name="property">The tracked parent property.</param>
    /// <param name="unitCode">The backend-generated room/unit business code.</param>
    /// <param name="fields">The edited room fields with parent floor number.</param>
    /// <param name="lookups">The resolved lookup identifiers for the room.</param>
    /// <returns>The new unit entity.</returns>
    private static Unit CreateUnit(
        Property property,
        string unitCode,
        RoomFieldUpdateModel fields,
        RoomFieldLookupModel lookups)
    {
        // Create backend-owned identity fields first, then reuse the same partial room mapper as room updates.
        var unit = new Unit
        {
            PublicId = Guid.NewGuid(),
            Property = property,
            UnitCode = unitCode,
            StatusId = lookups.AvailableStatusId.GetValueOrDefault(),
            IsPublished = false
        };

        RoomFieldMutationMapper.Apply(unit, fields, lookups);
        return unit;
    }

    /// <summary>
    /// Synchronizes submitted property-level charge policies.
    /// </summary>
    /// <param name="property">The tracked property graph.</param>
    /// <param name="chargePolicies">The prepared common charge policy mutations.</param>
    /// <param name="cancellationToken">The token used to cancel the staged mutation.</param>
    private async Task SyncChargePoliciesAsync(
        Property property,
        IReadOnlyList<PropertyChargePolicyMutationModel> chargePolicies,
        CancellationToken cancellationToken)
    {
        // Isolate common property policies so room-level overrides are never updated by this property flow.
        var commonPolicies = property.RentalChargePolicies
            .Where(policy => policy.UnitId is null)
            .ToList();
        var existingById = commonPolicies.ToDictionary(policy => policy.PublicId);

        foreach (var mutation in chargePolicies)
        {
            var requestPolicy = mutation.Policy;

            if (requestPolicy.Id.HasValue)
            {
                // Existing policy ids were guarded earlier; keep the mutation step explicit for analyzer safety.
                if (!existingById.TryGetValue(requestPolicy.Id.Value, out var policy))
                {
                    throw new ApiException(
                        ApplicationErrorConstants.PropertyErrors.ERROR_PROPERTY_CHARGE_POLICY_NOT_FOUND,
                        NOT_FOUND,
                        StatusCodes.Status404NotFound);
                }

                RentalChargePolicyFieldMapper.Apply(policy, requestPolicy, mutation.Lookups);
                await _rentalChargePolicyRepository.UpdateAsync(policy);
                continue;
            }

            // A policy without an id is a new property-level common policy; omitted policies remain unchanged.
            var newPolicy = CreateChargePolicy(property, mutation);
            property.RentalChargePolicies.Add(newPolicy);
            await _rentalChargePolicyRepository.AddAsync(newPolicy, cancellationToken);
        }
    }

    /// <summary>
    /// Creates a new property-level charge policy.
    /// </summary>
    /// <param name="property">The tracked parent property.</param>
    /// <param name="mutation">The edited policy and its resolved persistence identifiers.</param>
    /// <returns>The new charge policy entity.</returns>
    private static RentalChargePolicy CreateChargePolicy(
        Property property,
        PropertyChargePolicyMutationModel mutation)
    {
        var policy = new RentalChargePolicy
        {
            PublicId = Guid.NewGuid(),
            Property = property
        };
        RentalChargePolicyFieldMapper.Apply(policy, mutation.Policy, mutation.Lookups);
        return policy;
    }

    /// <summary>
    /// Synchronizes default and requested common package templates for the property.
    /// </summary>
    /// <param name="property">The tracked property graph.</param>
    /// <param name="packageTemplates">The edited package templates.</param>
    /// <param name="lookups">The package lookup values required by synchronization.</param>
    /// <param name="cancellationToken">The token used to cancel the staged mutation.</param>
    private async Task SyncPackagesAsync(
        Property property,
        IReadOnlyList<PropertyUpdatePackageModel> packageTemplates,
        UnitPackageMutationLookupModel lookups,
        CancellationToken cancellationToken)
    {
        // Every common setup keeps the no-furniture option, even when no custom package is submitted.
        await EnsureDefaultPackageAsync(property, lookups, cancellationToken);

        foreach (var packageTemplate in packageTemplates)
        {
            await SyncPropertyPackageAsync(property, packageTemplate, lookups, cancellationToken);
        }
    }

    /// <summary>
    /// Ensures the property has the default no-furniture common package.
    /// </summary>
    /// <param name="property">The tracked property entity.</param>
    /// <param name="lookups">The package lookup values required by synchronization.</param>
    /// <param name="cancellationToken">The token used to cancel the staged mutation.</param>
    private async Task EnsureDefaultPackageAsync(
        Property property,
        UnitPackageMutationLookupModel lookups,
        CancellationToken cancellationToken)
    {
        var defaultPackage = property.UnitPackages.FirstOrDefault(unitPackage =>
            unitPackage.UnitId is null
            && string.Equals(
                unitPackage.PackageCode,
                MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE,
                StringComparison.OrdinalIgnoreCase));
        var defaultType = lookups.NoFurniturePackageType;

        if (defaultPackage is null)
        {
            // Create the invariant once at property level so rooms using common setup inherit it.
            defaultPackage = new UnitPackage
            {
                PublicId = Guid.NewGuid(),
                Property = property,
                PackageCode = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE
            };
            property.UnitPackages.Add(defaultPackage);
            await _unitPackageRepository.AddAsync(defaultPackage, cancellationToken);
        }

        defaultPackage.PackageTypeId = defaultType.Id;
        defaultPackage.PackageName = defaultType.Name;
        defaultPackage.Description = defaultType.Description;
        defaultPackage.PriceAdjustment = 0;
        defaultPackage.StatusId = lookups.ActiveStatusId;
    }

    /// <summary>
    /// Synchronizes one requested common package template on the property.
    /// </summary>
    /// <param name="property">The tracked property entity.</param>
    /// <param name="packageTemplate">The edited package template.</param>
    /// <param name="lookups">The package lookup values required by synchronization.</param>
    /// <param name="cancellationToken">The token used to cancel the staged mutation.</param>
    private async Task SyncPropertyPackageAsync(
        Property property,
        PropertyUpdatePackageModel packageTemplate,
        UnitPackageMutationLookupModel lookups,
        CancellationToken cancellationToken)
    {
        var unitPackage = packageTemplate.Package.Id.HasValue
            ? property.UnitPackages.FirstOrDefault(package =>
                package.UnitId is null && package.PublicId == packageTemplate.Package.Id.Value)
            : null;

        if (packageTemplate.Package.Id.HasValue && unitPackage is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.RoomErrors.ERROR_ROOM_PACKAGE_NOT_FOUND,
                BAD_REQUEST,
                StatusCodes.Status400BadRequest);
        }

        var isNewPackage = unitPackage is null;

        if (isNewPackage)
        {
            // New common packages receive internal codes; existing packages are matched only by public id.
            unitPackage = new UnitPackage
            {
                PublicId = Guid.NewGuid(),
                Property = property,
                PackageCode = CodeGenerationHelper.GenerateCode(
                    UNIT_PACKAGE_CODE_PREFIX,
                    UNIT_PACKAGE_CODE_RANDOM_LENGTH)
            };
            property.UnitPackages.Add(unitPackage);
            await _unitPackageRepository.AddAsync(unitPackage, cancellationToken);
        }

        ApplyPackageFields(unitPackage, packageTemplate, lookups, isNewPackage);
        await _unitPackageRepository.UpdateAsync(unitPackage);

        if (packageTemplate.Package.Items is not null)
        {
            // Null keeps current items; an empty submitted collection intentionally clears them.
            await _unitPackageItemRepository.ReplaceItemsAsync(
                unitPackage,
                packageTemplate.Package.Items.Select(item => item.Name).ToList(),
                cancellationToken);
        }
    }

    /// <summary>
    /// Applies editable package fields to a tracked unit package.
    /// </summary>
    /// <param name="unitPackage">The tracked unit package.</param>
    /// <param name="packageTemplate">The edited package template.</param>
    /// <param name="lookups">The package lookup values required by the mapped entity.</param>
    /// <param name="isNewPackage">Whether the package was created by the current mutation.</param>
    private static void ApplyPackageFields(
        UnitPackage unitPackage,
        PropertyUpdatePackageModel packageTemplate,
        UnitPackageMutationLookupModel lookups,
        bool isNewPackage)
    {
        // Mapster merges only submitted package fields; lookup-backed fields remain service-owned.
        packageTemplate.Package.Adapt(unitPackage);
        if (isNewPackage)
        {
            // Internal type and status are creation defaults; partial updates preserve persisted values.
            unitPackage.PackageTypeId = lookups.CustomPackageTypeId;
            unitPackage.StatusId = lookups.ActiveStatusId;
        }
    }

    /// <summary>
    /// Builds the exact master-data keys needed by property creation.
    /// </summary>
    /// <param name="request">The property creation request.</param>
    /// <returns>The master-data keys required by the create flow.</returns>
    private static IReadOnlyCollection<MasterDataKeyModel> GetCreateMasterDataKeys(PropertyCreationRequestModel request)
    {
        var keys = new HashSet<MasterDataKeyModel>
        {
            new(MasterDataTypeEnum.PropertyType, request.PropertyTypeCode),
            new(MasterDataTypeEnum.PropertyStatus, MASTER_CODE_PROPERTY_STATUS_DRAFT),
            new(MasterDataTypeEnum.UnitStatus, MASTER_CODE_UNIT_STATUS_AVAILABLE),
            new(MasterDataTypeEnum.UnitPackageType, MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE),
            new(MasterDataTypeEnum.UnitPackageStatus, MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE),
            new(MasterDataTypeEnum.PropertyRelationshipType, PropertyRelationshipTypeEnum.Landlord.ToMasterDataCode()),
            new(MasterDataTypeEnum.CommonStatus, MASTER_CODE_ACTIVE)
        };

        // FE sends final room rows, so resolve only the unit type/mode codes present in that payload.
        foreach (var room in request.StructureSetup.Floors.SelectMany(floor => floor.Rooms ?? []))
        {
            keys.Add(new MasterDataKeyModel(MasterDataTypeEnum.UnitType, room.TypeCode));
            keys.Add(new MasterDataKeyModel(MasterDataTypeEnum.UnitRentalMode, room.RentalModeCode));
        }

        if ((request.Packages ?? []).Count > 0)
        {
            keys.Add(new MasterDataKeyModel(MasterDataTypeEnum.UnitPackageType, MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM));
        }

        AddChargePolicyMasterDataKeys(keys, request.ChargePolicies);

        return keys;
    }

    /// <summary>
    /// Builds typed lookup values for property creation graph mapping.
    /// </summary>
    /// <param name="request">The validated property creation request.</param>
    /// <param name="masterData">The resolved exact master-data values.</param>
    /// <returns>The typed lookup model used by the graph mapper.</returns>
    private static PropertyCreationLookupModel BuildCreateLookups(
        PropertyCreationRequestModel request,
        IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> masterData)
    {
        // Start from the exact submitted room and charge-policy rows so no unrelated master-data value is loaded.
        var roomRows = request.StructureSetup.Floors.SelectMany(floor => floor.Rooms ?? []).ToList();
        var chargePolicies = request.ChargePolicies ?? [];

        // Resolve property creation defaults that every new property graph requires.
        return new PropertyCreationLookupModel
        {
            PropertyTypeId = masterData.GetValue(MasterDataTypeEnum.PropertyType, request.PropertyTypeCode).Id,
            PropertyStatusId = masterData.GetValue(
                MasterDataTypeEnum.PropertyStatus,
                MASTER_CODE_PROPERTY_STATUS_DRAFT).Id,
            UnitStatusId = masterData.GetValue(MasterDataTypeEnum.UnitStatus, MASTER_CODE_UNIT_STATUS_AVAILABLE).Id,
            ActiveStatusId = masterData.GetValue(MasterDataTypeEnum.CommonStatus, MASTER_CODE_ACTIVE).Id,
            LandlordRelationshipTypeId = masterData.GetValue(
                MasterDataTypeEnum.PropertyRelationshipType,
                PropertyRelationshipTypeEnum.Landlord.ToMasterDataCode()).Id,
            NoFurniturePackageType = masterData.GetValue(
                MasterDataTypeEnum.UnitPackageType,
                MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE),
            CustomPackageType = (request.Packages ?? []).Count > 0
                ? masterData.GetValue(MasterDataTypeEnum.UnitPackageType, MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM)
                : null,
            PackageStatus = masterData.GetValue(
                MasterDataTypeEnum.UnitPackageStatus,
                MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE),
            // Build room type and rental-mode identifiers from the submitted final room structure.
            UnitTypeIds = roomRows
                .Select(room => room.TypeCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    code => code,
                    code => masterData.GetValue(MasterDataTypeEnum.UnitType, code).Id,
                    StringComparer.OrdinalIgnoreCase),
            RentalModeIds = roomRows
                .Select(room => room.RentalModeCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    code => code,
                    code => masterData.GetValue(MasterDataTypeEnum.UnitRentalMode, code).Id,
                    StringComparer.OrdinalIgnoreCase),
            // Resolve charge lines and parking vehicle types only for submitted common policies.
            ChargeTypes = chargePolicies
                .Select(policy => policy.Code)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    code => code,
                    code => masterData.GetValue(MasterDataTypeEnum.InvoiceLineType, code),
                    StringComparer.OrdinalIgnoreCase),
            VehicleTypeIds = chargePolicies
                .Where(policy => string.Equals(
                    policy.Code,
                    MASTER_CODE_INVOICE_LINE_TYPE_PARKING,
                    StringComparison.OrdinalIgnoreCase))
                .Select(policy => policy.VehicleTypeCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    code => code,
                    code => masterData.GetValue(MasterDataTypeEnum.VehicleType, code).Id,
                    StringComparer.OrdinalIgnoreCase)
        };
    }

    /// <summary>
    /// Builds the exact master-data keys needed by property update.
    /// </summary>
    /// <param name="request">The property update request.</param>
    /// <returns>The master-data keys required by the update flow.</returns>
    private static IReadOnlyCollection<MasterDataKeyModel> GetUpdateMasterDataKeys(PropertyUpdateRequestModel request)
    {
        var keys = new HashSet<MasterDataKeyModel>();

        if (request.PropertyTypeCode is not null)
        {
            keys.Add(new MasterDataKeyModel(MasterDataTypeEnum.PropertyType, request.PropertyTypeCode));
        }

        // Existing room updates resolve only submitted type/mode codes; new rooms also need the available status.
        foreach (var room in (request.Structure?.Floors ?? []).SelectMany(floor => floor.Rooms ?? []))
        {
            if (!room.Id.HasValue)
            {
                keys.Add(new MasterDataKeyModel(MasterDataTypeEnum.UnitStatus, MASTER_CODE_UNIT_STATUS_AVAILABLE));
            }

            if (room.TypeCode is not null)
            {
                keys.Add(new MasterDataKeyModel(MasterDataTypeEnum.UnitType, room.TypeCode));
            }

            if (room.RentalModeCode is not null)
            {
                keys.Add(new MasterDataKeyModel(MasterDataTypeEnum.UnitRentalMode, room.RentalModeCode));
            }
        }

        // Collect policy lookups only when the common policy section is part of this partial update.
        if (request.ChargePolicies is not null)
        {
            if (request.ChargePolicies.Any(policy => !policy.Id.HasValue))
            {
                keys.Add(new MasterDataKeyModel(MasterDataTypeEnum.CommonStatus, MASTER_CODE_ACTIVE));
            }

            AddChargePolicyMasterDataKeys(keys, request.ChargePolicies);
        }

        // Package updates always require default metadata and need the custom type only for new templates.
        if (request.Packages is not null)
        {
            keys.Add(new MasterDataKeyModel(
                MasterDataTypeEnum.UnitPackageType,
                MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE));
            keys.Add(new MasterDataKeyModel(
                MasterDataTypeEnum.UnitPackageStatus,
                MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE));
            if (request.Packages.Any(package => !package.Id.HasValue))
            {
                keys.Add(new MasterDataKeyModel(
                    MasterDataTypeEnum.UnitPackageType,
                    MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM));
            }
        }

        return keys;
    }

    /// <summary>
    /// Adds master-data keys for charge policies and parking vehicle types.
    /// </summary>
    /// <param name="keys">The key set being built.</param>
    /// <param name="chargePolicies">The charge policy payload.</param>
    private static void AddChargePolicyMasterDataKeys(
        ISet<MasterDataKeyModel> keys,
        IReadOnlyCollection<IChargePolicyInput> chargePolicies)
    {
        // Scan every submitted policy once because charge line and parking vehicle type use separate master-data types.
        foreach (var chargePolicy in chargePolicies ?? [])
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
    }

}
