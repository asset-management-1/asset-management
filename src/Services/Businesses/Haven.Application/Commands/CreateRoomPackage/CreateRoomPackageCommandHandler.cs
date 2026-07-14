namespace Haven.Application.Commands.CreateRoomPackage;

/// <summary>
/// Coordinates landlord-scoped room package creation.
/// </summary>
public sealed class CreateRoomPackageCommandHandler
    : ICommandHandler<CreateRoomPackageCommand, ResponseDto<RoomPackageDetailResponseDto>>
{
    private readonly IRoomPackageService _roomPackageService;
    private readonly IPartyService _partyService;
    private readonly ILogger<CreateRoomPackageCommandHandler> _logger;

    /// <summary>
    /// Creates the handler that resolves landlord context and delegates package creation.
    /// </summary>
    /// <param name="roomPackageService">The room package use-case service.</param>
    /// <param name="partyService">The service used to resolve the active landlord context.</param>
    /// <param name="logger">The structured command workflow logger.</param>
    public CreateRoomPackageCommandHandler(
        IRoomPackageService roomPackageService,
        IPartyService partyService,
        ILogger<CreateRoomPackageCommandHandler> logger)
    {
        _roomPackageService = roomPackageService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a package under the current landlord's room scope.
    /// </summary>
    /// <param name="request">The validated room package creation command.</param>
    /// <param name="cancellationToken">The token used to cancel context resolution and package creation.</param>
    /// <returns>The created room package detail inside the standard response envelope.</returns>
    public async ValueTask<ResponseDto<RoomPackageDetailResponseDto>> Handle(
        CreateRoomPackageCommand request,
        CancellationToken cancellationToken)
    {
        // Resolve landlord context before mapping the public command into the package workflow request.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var serviceRequest = request.Adapt<RoomPackageCreateRequestModel>();
        serviceRequest.CurrentParty = currentParty;

        // The service owns materialization, package invariants, and the transaction.
        var response = await _roomPackageService.CreateAsync(serviceRequest, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.RoomPackageLogs.ROOM_PACKAGE_CREATE_COMMAND_COMPLETED,
            response.Package.Id,
            request.RoomId,
            currentParty.PartyPublicId);

        return new ResponseDto<RoomPackageDetailResponseDto>(response);
    }
}
