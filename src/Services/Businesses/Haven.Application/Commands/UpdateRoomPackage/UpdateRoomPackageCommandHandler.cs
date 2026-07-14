namespace Haven.Application.Commands.UpdateRoomPackage;

/// <summary>
/// Coordinates landlord-scoped room package updates.
/// </summary>
public sealed class UpdateRoomPackageCommandHandler
    : ICommandHandler<UpdateRoomPackageCommand, ResponseDto<RoomPackageDetailResponseDto>>
{
    private readonly IRoomPackageService _roomPackageService;
    private readonly IPartyService _partyService;
    private readonly ILogger<UpdateRoomPackageCommandHandler> _logger;

    /// <summary>
    /// Creates the handler that scopes and delegates partial room package updates.
    /// </summary>
    /// <param name="roomPackageService">The room package use-case service.</param>
    /// <param name="partyService">The service used to resolve the active landlord context.</param>
    /// <param name="logger">The structured command workflow logger.</param>
    public UpdateRoomPackageCommandHandler(
        IRoomPackageService roomPackageService,
        IPartyService partyService,
        ILogger<UpdateRoomPackageCommandHandler> logger)
    {
        _roomPackageService = roomPackageService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Applies submitted fields to one effective room package.
    /// </summary>
    /// <param name="request">The validated partial package update command.</param>
    /// <param name="cancellationToken">The token used to cancel context resolution and package mutation.</param>
    /// <returns>The resulting room-owned package detail inside the standard response envelope.</returns>
    public async ValueTask<ResponseDto<RoomPackageDetailResponseDto>> Handle(
        UpdateRoomPackageCommand request,
        CancellationToken cancellationToken)
    {
        // Resolve landlord scope and map only public update fields into the typed service request.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var serviceRequest = request.Adapt<RoomPackageUpdateRequestModel>();
        serviceRequest.CurrentParty = currentParty;

        // The service materializes common packages when needed and returns the resulting room-level identifier.
        var response = await _roomPackageService.UpdateAsync(serviceRequest, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.RoomPackageLogs.ROOM_PACKAGE_UPDATE_COMMAND_COMPLETED,
            response.Package.Id,
            request.RoomId,
            currentParty.PartyPublicId);

        return new ResponseDto<RoomPackageDetailResponseDto>(response);
    }
}
