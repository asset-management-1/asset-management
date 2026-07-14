namespace Haven.Application.Queries.GetRoomPackages;

/// <summary>
/// Coordinates landlord-scoped room package list reads.
/// </summary>
public sealed class GetRoomPackagesQueryHandler
    : IQueryHandler<GetRoomPackagesQuery, ResponseDto<RoomPackageListResponseDto>>
{
    private readonly IRoomPackageService _roomPackageService;
    private readonly IPartyService _partyService;
    private readonly ILogger<GetRoomPackagesQueryHandler> _logger;

    /// <summary>
    /// Creates the handler that scopes effective package list reads to the current landlord.
    /// </summary>
    /// <param name="roomPackageService">The room package read service.</param>
    /// <param name="partyService">The service used to resolve the active landlord context.</param>
    /// <param name="logger">The structured query workflow logger.</param>
    public GetRoomPackagesQueryHandler(
        IRoomPackageService roomPackageService,
        IPartyService partyService,
        ILogger<GetRoomPackagesQueryHandler> logger)
    {
        _roomPackageService = roomPackageService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Loads the effective package set for one room.
    /// </summary>
    /// <param name="request">The validated room package list query.</param>
    /// <param name="cancellationToken">The token used to cancel context resolution and the scoped read.</param>
    /// <returns>The effective user-managed package list inside the standard response envelope.</returns>
    public async ValueTask<ResponseDto<RoomPackageListResponseDto>> Handle(
        GetRoomPackagesQuery request,
        CancellationToken cancellationToken)
    {
        // Resolve current landlord scope before querying packages that may come from the property fallback.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var response = await _roomPackageService.GetListAsync(
            new RoomPackageListRequestModel
            {
                CurrentParty = currentParty,
                RoomPublicId = request.RoomId
            },
            cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.RoomPackageLogs.ROOM_PACKAGE_LIST_QUERY_COMPLETED,
            request.RoomId,
            currentParty.PartyPublicId);

        return new ResponseDto<RoomPackageListResponseDto>(response);
    }
}
