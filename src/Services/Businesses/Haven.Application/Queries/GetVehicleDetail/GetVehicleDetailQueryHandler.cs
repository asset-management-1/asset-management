namespace Haven.Application.Queries.GetVehicleDetail;

/// <summary>
/// Coordinates landlord vehicle-detail reads.
/// </summary>
public class GetVehicleDetailQueryHandler : IQueryHandler<GetVehicleDetailQuery, ResponseDto<VehicleDetailResponseDto>>
{
    private readonly IVehicleService _vehicleService;
    private readonly IPartyService _partyService;
    private readonly ILogger<GetVehicleDetailQueryHandler> _logger;

    /// <summary>
    /// Creates the vehicle detail query handler.
    /// </summary>
    /// <param name="vehicleService">The vehicle detail workflow service.</param>
    /// <param name="partyService">The service used to resolve the current landlord.</param>
    /// <param name="logger">The structured query logger.</param>
    public GetVehicleDetailQueryHandler(IVehicleService vehicleService, IPartyService partyService, ILogger<GetVehicleDetailQueryHandler> logger)
    {
        _vehicleService = vehicleService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Loads a vehicle detail under the current landlord scope.
    /// </summary>
    /// <param name="request">The vehicle detail query.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The vehicle detail response envelope.</returns>
    public async ValueTask<ResponseDto<VehicleDetailResponseDto>> Handle(GetVehicleDetailQuery request, CancellationToken cancellationToken)
    {
        // Resolve the current party before the scoped vehicle detail query.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var response = await _vehicleService.GetDetailAsync(new VehicleDetailRequestModel
        {
            CurrentParty = currentParty,
            RoomPublicId = request.RoomId,
            VehiclePublicId = request.VehiclePublicId
        }, cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.VehicleLogs.VEHICLE_DETAIL_QUERY_COMPLETED, request.VehiclePublicId, currentParty.PartyPublicId);

        return new ResponseDto<VehicleDetailResponseDto>(response);
    }
}
