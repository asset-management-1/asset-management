namespace Haven.Application.Queries.GetRooms;

/// <summary>
/// Coordinates landlord room ledger reads for the room management screen.
/// </summary>
public class GetRoomsQueryHandler : IQueryHandler<GetRoomsQuery, ResponseDto<PaginationResponse<IReadOnlyList<RoomListPropertyResponseDto>>>>
{
    private readonly IRoomService _roomService;
    private readonly IPartyService _partyService;
    private readonly ILogger<GetRoomsQueryHandler> _logger;

    /// <summary>
    /// Creates the room list query handler.
    /// </summary>
    /// <param name="roomService">The room service.</param>
    /// <param name="partyService">The current party service.</param>
    /// <param name="logger">The logger used for query completion tracking.</param>
    public GetRoomsQueryHandler(
        IRoomService roomService,
        IPartyService partyService,
        ILogger<GetRoomsQueryHandler> logger)
    {
        _roomService = roomService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Loads grouped rooms for the current party.
    /// </summary>
    /// <param name="request">The room list query.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The standardized paged room response.</returns>
    public async ValueTask<ResponseDto<PaginationResponse<IReadOnlyList<RoomListPropertyResponseDto>>>> Handle(
        GetRoomsQuery request,
        CancellationToken cancellationToken)
    {
        // Application boundary resolves current party before shaping the read request.
        var currentParty = await _partyService.GetCurrentPartyAsync(cancellationToken);
        var roomRequest = request.Adapt<RoomListRequestModel>();
        roomRequest.CurrentParty = currentParty;

        // Infrastructure service owns Dapper reads and response grouping.
        var response = await _roomService.GetRoomsAsync(roomRequest, cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.RoomLogs.ROOM_LIST_QUERY_COMPLETED, currentParty.PartyPublicId);

        return new ResponseDto<PaginationResponse<IReadOnlyList<RoomListPropertyResponseDto>>>(response);
    }
}
