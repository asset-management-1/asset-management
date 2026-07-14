namespace Haven.Application.Queries.GetRoomDetail;

/// <summary>
/// Coordinates landlord room detail reads for edit and management screens.
/// </summary>
public class GetRoomDetailQueryHandler : IQueryHandler<GetRoomDetailQuery, ResponseDto<RoomDetailResponseDto>>
{
    private readonly IRoomService _roomService;
    private readonly IPartyService _partyService;
    private readonly ILogger<GetRoomDetailQueryHandler> _logger;

    /// <summary>
    /// Creates the room detail query handler.
    /// </summary>
    /// <param name="roomService">The room service.</param>
    /// <param name="partyService">The current party service.</param>
    /// <param name="logger">The logger used for query completion tracking.</param>
    public GetRoomDetailQueryHandler(
        IRoomService roomService,
        IPartyService partyService,
        ILogger<GetRoomDetailQueryHandler> logger)
    {
        _roomService = roomService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Loads one room detail under the current party scope.
    /// </summary>
    /// <param name="request">The room detail query.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The standardized room detail response.</returns>
    public async ValueTask<ResponseDto<RoomDetailResponseDto>> Handle(
        GetRoomDetailQuery request,
        CancellationToken cancellationToken)
    {
        // Application boundary resolves party before the service reads room detail.
        var currentParty = await _partyService.GetCurrentPartyAsync(cancellationToken);
        var roomRequest = new RoomDetailRequestModel
        {
            CurrentParty = currentParty,
            RoomPublicId = request.RoomPublicId
        };

        // The service composes room setup, tenants, vehicles, and effective pricing for the detail screen.
        var response = await _roomService.GetRoomDetailAsync(roomRequest, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.RoomLogs.ROOM_DETAIL_QUERY_COMPLETED,
            request.RoomPublicId,
            currentParty.PartyPublicId);

        return new ResponseDto<RoomDetailResponseDto>(response);
    }
}
