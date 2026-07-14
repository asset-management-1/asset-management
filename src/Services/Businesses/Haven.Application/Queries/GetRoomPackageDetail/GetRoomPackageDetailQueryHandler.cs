namespace Haven.Application.Queries.GetRoomPackageDetail;

/// <summary>
/// Coordinates landlord-scoped room package detail reads.
/// </summary>
public sealed class GetRoomPackageDetailQueryHandler
    : IQueryHandler<GetRoomPackageDetailQuery, ResponseDto<RoomPackageDetailResponseDto>>
{
    private readonly IRoomPackageService _roomPackageService;
    private readonly IPartyService _partyService;
    private readonly ILogger<GetRoomPackageDetailQueryHandler> _logger;

    /// <summary>
    /// Creates the handler that scopes effective package detail reads to the current landlord.
    /// </summary>
    /// <param name="roomPackageService">The room package read service.</param>
    /// <param name="partyService">The service used to resolve the active landlord context.</param>
    /// <param name="logger">The structured query workflow logger.</param>
    public GetRoomPackageDetailQueryHandler(
        IRoomPackageService roomPackageService,
        IPartyService partyService,
        ILogger<GetRoomPackageDetailQueryHandler> logger)
    {
        _roomPackageService = roomPackageService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Loads one package only when it belongs to the room's current effective set.
    /// </summary>
    /// <param name="request">The validated package and room identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel context resolution and the scoped detail read.</param>
    /// <returns>The effective package detail inside the standard response envelope.</returns>
    public async ValueTask<ResponseDto<RoomPackageDetailResponseDto>> Handle(
        GetRoomPackageDetailQuery request,
        CancellationToken cancellationToken)
    {
        // Resolve landlord scope and pass both identifiers because common packages are interpreted per room.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var response = await _roomPackageService.GetDetailAsync(
            new RoomPackageDetailRequestModel
            {
                CurrentParty = currentParty,
                RoomPublicId = request.RoomPublicId,
                PackagePublicId = request.PackagePublicId
            },
            cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.RoomPackageLogs.ROOM_PACKAGE_DETAIL_QUERY_COMPLETED,
            request.PackagePublicId,
            request.RoomPublicId,
            currentParty.PartyPublicId);

        return new ResponseDto<RoomPackageDetailResponseDto>(response);
    }
}
