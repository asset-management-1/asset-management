namespace Haven.Application.Queries.GetVehicles;

/// <summary>
/// Coordinates landlord-scoped vehicle management list reads.
/// </summary>
public sealed class GetVehiclesQueryHandler
    : IQueryHandler<GetVehiclesQuery, ResponseDto<IReadOnlyList<VehicleListItemResponseDto>>>
{
    private readonly IVehicleService _vehicleService;
    private readonly IPartyService _partyService;
    private readonly ILogger<GetVehiclesQueryHandler> _logger;

    /// <summary>
    /// Creates the room vehicle list query handler.
    /// </summary>
    /// <param name="vehicleService">The service that loads the room-scoped vehicle collection.</param>
    /// <param name="partyService">The service that resolves the current landlord context.</param>
    /// <param name="logger">The query workflow logger.</param>
    public GetVehiclesQueryHandler(
        IVehicleService vehicleService,
        IPartyService partyService,
        ILogger<GetVehiclesQueryHandler> logger)
    {
        _vehicleService = vehicleService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Loads every active vehicle for a room managed by the current landlord.
    /// </summary>
    /// <param name="request">The required room identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The standardized room vehicle collection.</returns>
    public async ValueTask<ResponseDto<IReadOnlyList<VehicleListItemResponseDto>>> Handle(
        GetVehiclesQuery request,
        CancellationToken cancellationToken)
    {
        // Resolve landlord scope before mapping the room identifier into the service request.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var serviceRequest = request.Adapt<VehicleListRequestModel>();
        serviceRequest.CurrentParty = currentParty;

        // The service delegates the bounded room projection and excludes detail-only image fields.
        var response = await _vehicleService.GetVehiclesAsync(serviceRequest, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.VehicleLogs.VEHICLE_LIST_QUERY_COMPLETED,
            currentParty.PartyPublicId);

        return new ResponseDto<IReadOnlyList<VehicleListItemResponseDto>>(response);
    }
}
