namespace Haven.Application.Commands.DeleteRoomPackage;

/// <summary>
/// Coordinates landlord-scoped room package deletion.
/// </summary>
public sealed class DeleteRoomPackageCommandHandler
    : ICommandHandler<DeleteRoomPackageCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IRoomPackageService _roomPackageService;
    private readonly IPartyService _partyService;
    private readonly ILogger<DeleteRoomPackageCommandHandler> _logger;

    /// <summary>
    /// Creates the handler that scopes and delegates contract-protected package deletion.
    /// </summary>
    /// <param name="roomPackageService">The room package use-case service.</param>
    /// <param name="partyService">The service used to resolve the active landlord context.</param>
    /// <param name="logger">The structured command workflow logger.</param>
    public DeleteRoomPackageCommandHandler(
        IRoomPackageService roomPackageService,
        IPartyService partyService,
        ILogger<DeleteRoomPackageCommandHandler> logger)
    {
        _roomPackageService = roomPackageService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Deletes one package when contract history permits the mutation.
    /// </summary>
    /// <param name="request">The validated package and room identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel context resolution and package deletion.</param>
    /// <returns>The successful operation result inside the standard response envelope.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        DeleteRoomPackageCommand request,
        CancellationToken cancellationToken)
    {
        // Resolve landlord scope before invoking the contract-protected package mutation.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var response = await _roomPackageService.DeleteAsync(
            new RoomPackageDeleteRequestModel
            {
                CurrentParty = currentParty,
                RoomPublicId = request.RoomPublicId,
                PackagePublicId = request.PackagePublicId
            },
            cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.RoomPackageLogs.ROOM_PACKAGE_DELETE_COMMAND_COMPLETED,
            request.PackagePublicId,
            request.RoomPublicId,
            currentParty.PartyPublicId);

        return new ResponseDto<OperationStatusResponseDto>(response);
    }
}
