namespace Haven.Application.Queries.GetRoomMeterPeriod;

/// <summary>
/// Handles one room meter period query.
/// </summary>
public class GetRoomMeterPeriodQueryHandler
    : IQueryHandler<GetRoomMeterPeriodQuery, ResponseDto<MeterPeriodDetailResponseDto>>
{
    private readonly IRoomMeterService _meterService;
    private readonly IPartyService _partyService;
    private readonly ILogger<GetRoomMeterPeriodQueryHandler> _logger;

    /// <summary>
    /// Creates the room meter period handler.
    /// </summary>
    /// <param name="meterService">The room meter read service.</param>
    /// <param name="partyService">The current landlord resolver.</param>
    /// <param name="logger">The structured query logger.</param>
    public GetRoomMeterPeriodQueryHandler(
        IRoomMeterService meterService,
        IPartyService partyService,
        ILogger<GetRoomMeterPeriodQueryHandler> logger)
    {
        _meterService = meterService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Loads one landlord-scoped meter month with its live invoice impact.
    /// </summary>
    /// <param name="request">The validated room and calendar query.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The selected room meter period response.</returns>
    public async ValueTask<ResponseDto<MeterPeriodDetailResponseDto>> Handle(
        GetRoomMeterPeriodQuery request,
        CancellationToken cancellationToken)
    {
        // Resolve landlord scope before converting the public room identifier into internal query scope.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var serviceRequest = request.Adapt<MeterPeriodRequestModel>();
        serviceRequest.CurrentParty = currentParty;

        // The service combines persisted values, prior baselines, effective prices, and invoice state.
        var response = await _meterService.GetPeriodAsync(serviceRequest, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.MeterLogs.PERIOD_LOADED,
            request.RoomId,
            request.Month,
            request.Year);

        return new ResponseDto<MeterPeriodDetailResponseDto>(response);
    }
}
