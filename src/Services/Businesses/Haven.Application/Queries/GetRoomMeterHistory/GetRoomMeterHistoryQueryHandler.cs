namespace Haven.Application.Queries.GetRoomMeterHistory;

/// <summary>
/// Handles bounded room meter history queries.
/// </summary>
public class GetRoomMeterHistoryQueryHandler
    : IQueryHandler<GetRoomMeterHistoryQuery, ResponseDto<IReadOnlyList<MeterHistoryItemResponseDto>>>
{
    private readonly IRoomMeterService _meterService;
    private readonly IPartyService _partyService;
    private readonly ILogger<GetRoomMeterHistoryQueryHandler> _logger;

    /// <summary>
    /// Creates the room meter history handler.
    /// </summary>
    /// <param name="meterService">The room meter read service.</param>
    /// <param name="partyService">The current landlord resolver.</param>
    /// <param name="logger">The structured query logger.</param>
    public GetRoomMeterHistoryQueryHandler(
        IRoomMeterService meterService,
        IPartyService partyService,
        ILogger<GetRoomMeterHistoryQueryHandler> logger)
    {
        _meterService = meterService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Loads at most twelve meter periods for the selected room and year.
    /// </summary>
    /// <param name="request">The validated room and year query.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The room meter history response.</returns>
    public async ValueTask<ResponseDto<IReadOnlyList<MeterHistoryItemResponseDto>>> Handle(
        GetRoomMeterHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // Resolve landlord scope before converting the public room identifier into internal query scope.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var serviceRequest = request.Adapt<MeterHistoryRequestModel>();
        serviceRequest.CurrentParty = currentParty;

        // The service groups typed electricity and water rows by calendar month.
        var response = await _meterService.GetHistoryAsync(serviceRequest, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.MeterLogs.HISTORY_LOADED,
            request.RoomId,
            request.Year);

        return new ResponseDto<IReadOnlyList<MeterHistoryItemResponseDto>>(response);
    }
}
