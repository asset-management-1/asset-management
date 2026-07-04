namespace Haven.Infrastructure.Services;

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
    /// <param name="cancellationToken">The token used to cancel the read.</param>
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

        var floorRowsTask = GetFloorRowsAsync(childParameters, cancellationToken);
        var roomRowsTask = GetRoomRowsAsync(childParameters, cancellationToken);
        var roomTenantRowsTask = GetRoomTenantRowsAsync(childParameters, cancellationToken);

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
    /// <param name="cancellationToken">The token used to cancel the read.</param>
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
            throw new ApiException(ERROR_PROPERTY_NOT_FOUND, NOT_FOUND, StatusCodes.Status404NotFound);
        }

        // Detail uses the same derived floor/room shape as list, plus policy and management cards.
        var childParameters = new PropertyChildRowsQueryParametersModel
        {
            PropertyPublicIds = [propertyRow.PropertyPublicId]
        };
        var floorRowsTask = GetFloorRowsAsync(childParameters, cancellationToken);
        var roomRowsTask = GetRoomRowsAsync(childParameters, cancellationToken);
        var chargePoliciesTask = _propertyRepository.GetChargePoliciesAsync(request.PropertyPublicId, cancellationToken);
        var packageTemplatesTask = _propertyRepository.GetPackageTemplatesAsync(request.PropertyPublicId, cancellationToken);
        var managementSummaryTask = _propertyRepository.GetManagementSummaryAsync(request.PropertyPublicId, cancellationToken);

        await Task.WhenAll(floorRowsTask, roomRowsTask, chargePoliciesTask, packageTemplatesTask, managementSummaryTask);

        var floorRows = await floorRowsTask;
        var roomRows = await roomRowsTask;
        var chargePolicies = await chargePoliciesTask;
        var packageTemplates = await packageTemplatesTask;
        var managementSummary = await managementSummaryTask;

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
            managementSummary);
    }

    /// <summary>
    /// Creates a property with generated unit structure, unit packages, and optional charge policies.
    /// </summary>
    /// <param name="request">The party-scoped property creation request.</param>
    /// <param name="cancellationToken">The token used to cancel the write.</param>
    /// <returns>The created property summary.</returns>
    public async Task<CreatedPropertyResponseDto> CreatePropertyAsync(
        PropertyCreationRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Application has shaped the request; service validates all master-data values needed for mapping.
        var masterDataKeys = request.GetMasterDataKeys();
        request.MasterData = await _masterDataService.GetValuesAsync(masterDataKeys, cancellationToken);

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
                    ERROR_PROPERTY_CODE_GENERATION_FAILED,
                    INTERNAL_SERVER,
                    StatusCodes.Status500InternalServerError)
            },
            cancellationToken);

        // Persist the property graph atomically so partial property/unit setup cannot leak to readers.
        return await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
        {
            // EF is used here because create must persist one mapped property graph atomically in a transaction.
            var graph = request.Adapt<PropertyCreationGraphModel>();

            await _propertyRepository.AddAsync(graph.Property, transactionToken);
            await _unitRepository.AddRangeAsync(graph.Units, transactionToken);
            await _unitPackageRepository.AddRangeAsync(graph.UnitPackages, transactionToken);
            if (graph.UnitPackageItems.Count > 0)
            {
                await _unitPackageItemRepository.AddRangeAsync(graph.UnitPackageItems, transactionToken);
            }

            await _propertyPartyRepository.AddAsync(graph.PropertyParty, transactionToken);

            if (graph.ChargePolicies.Count > 0)
            {
                await _rentalChargePolicyRepository.AddRangeAsync(graph.ChargePolicies, transactionToken);
            }

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
    /// Loads floors for selected property public identifiers.
    /// </summary>
    /// <param name="parameters">The property identifiers and optional child-row filters.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>Floor rows.</returns>
    private async Task<IReadOnlyList<PropertyFloorRowModel>> GetFloorRowsAsync(
        PropertyChildRowsQueryParametersModel parameters,
        CancellationToken cancellationToken)
    {
        // Skip child queries when the parent page has no properties.
        if (parameters.PropertyPublicIds.Count == 0)
        {
            return [];
        }

        // Floor rows are filtered with the same room/payment criteria used by the parent query.
        return await _propertyRepository.GetFloorsAsync(parameters, cancellationToken);
    }

    /// <summary>
    /// Loads room rows for selected property public identifiers.
    /// </summary>
    /// <param name="parameters">The property identifiers and optional child-row filters.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>Room rows.</returns>
    private async Task<IReadOnlyList<PropertyRoomRowModel>> GetRoomRowsAsync(
        PropertyChildRowsQueryParametersModel parameters,
        CancellationToken cancellationToken)
    {
        // Skip child queries when the parent page has no properties.
        if (parameters.PropertyPublicIds.Count == 0)
        {
            return [];
        }

        // Room rows are always loaded for the list API because it backs the Figma quick-room search screen.
        return await _propertyRepository.GetRoomsAsync(parameters, cancellationToken);
    }

    /// <summary>
    /// Loads active tenant rows for selected property public identifiers.
    /// </summary>
    /// <param name="parameters">The property identifiers and optional child-row filters.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>Room tenant rows.</returns>
    private async Task<IReadOnlyList<PropertyRoomTenantRowModel>> GetRoomTenantRowsAsync(
        PropertyChildRowsQueryParametersModel parameters,
        CancellationToken cancellationToken)
    {
        // Skip child queries when the parent page has no properties.
        if (parameters.PropertyPublicIds.Count == 0)
        {
            return [];
        }

        // Tenant rows feed every occupied room card through one consistent response shape.
        return await _propertyRepository.GetRoomTenantsAsync(parameters, cancellationToken);
    }
}
