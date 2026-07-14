namespace Haven.Application.Commands.DeleteRoom;

/// <summary>
/// Coordinates landlord room soft-delete requests.
/// </summary>
public class DeleteRoomCommandHandler : ICommandHandler<DeleteRoomCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IRoomService _roomService;
    private readonly IPartyService _partyService;
    private readonly ILogger<DeleteRoomCommandHandler> _logger;

    /// <summary>
    /// Creates the room delete handler.
    /// </summary>
    /// <param name="roomService">The room service.</param>
    /// <param name="partyService">The current landlord service.</param>
    /// <param name="logger">The logger used for command completion tracking.</param>
    public DeleteRoomCommandHandler(
        IRoomService roomService,
        IPartyService partyService,
        ILogger<DeleteRoomCommandHandler> logger)
    {
        _roomService = roomService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Soft-deletes one room when no active contract/occupancy blocks it.
    /// </summary>
    /// <param name="request">The room delete command.</param>
    /// <param name="cancellationToken">The token used to cancel the room deletion.</param>
    /// <returns>The standardized operation status response.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        DeleteRoomCommand request,
        CancellationToken cancellationToken)
    {
        // Deletes require landlord context because they mutate building-owned room setup.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var roomRequest = new RoomDeleteRequestModel
        {
            CurrentParty = currentParty,
            RoomPublicId = request.RoomPublicId
        };

        // The service blocks rooms with active rental data and performs the soft-delete transaction.
        var response = await _roomService.DeleteRoomAsync(roomRequest, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.RoomLogs.ROOM_DELETE_COMMAND_COMPLETED,
            request.RoomPublicId,
            currentParty.PartyPublicId);

        return new ResponseDto<OperationStatusResponseDto>(response);
    }
}
