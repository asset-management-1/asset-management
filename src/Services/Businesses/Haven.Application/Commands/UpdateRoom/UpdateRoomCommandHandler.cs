namespace Haven.Application.Commands.UpdateRoom;

/// <summary>
/// Coordinates landlord room setup updates.
/// </summary>
public class UpdateRoomCommandHandler : ICommandHandler<UpdateRoomCommand, ResponseDto<RoomDetailResponseDto>>
{
    private readonly IRoomService _roomService;
    private readonly IPartyService _partyService;
    private readonly ILogger<UpdateRoomCommandHandler> _logger;

    /// <summary>
    /// Creates the room update handler.
    /// </summary>
    /// <param name="roomService">The room service.</param>
    /// <param name="partyService">The current landlord service.</param>
    /// <param name="logger">The logger used for command completion tracking.</param>
    public UpdateRoomCommandHandler(
        IRoomService roomService,
        IPartyService partyService,
        ILogger<UpdateRoomCommandHandler> logger)
    {
        _roomService = roomService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Updates one room and returns refreshed room detail.
    /// </summary>
    /// <param name="request">The room update command.</param>
    /// <param name="cancellationToken">The token used to cancel the room mutation.</param>
    /// <returns>The standardized refreshed room detail response.</returns>
    public async ValueTask<ResponseDto<RoomDetailResponseDto>> Handle(
        UpdateRoomCommand request,
        CancellationToken cancellationToken)
    {
        // Updates require landlord context because they mutate building-owned room setup.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var roomRequest = request.Adapt<RoomUpdateRequestModel>();
        roomRequest.CurrentParty = currentParty;

        // The service enforces protected-field guards before syncing room overrides and returning detail.
        var response = await _roomService.UpdateRoomAsync(roomRequest, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.RoomLogs.ROOM_UPDATE_COMMAND_COMPLETED,
            request.Id,
            currentParty.PartyPublicId);

        return new ResponseDto<RoomDetailResponseDto>(response);
    }
}
