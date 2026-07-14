namespace Haven.Application.Queries.GetRoomTenantJoinQr;

/// <summary>
/// Coordinates landlord-scoped room tenant-join QR generation.
/// </summary>
public class GetRoomTenantJoinQrQueryHandler : IQueryHandler<GetRoomTenantJoinQrQuery, ResponseDto<TenantJoinQrResponseDto>>
{
    private readonly ITenantService _tenantService;
    private readonly IPartyService _partyService;
    private readonly ILogger<GetRoomTenantJoinQrQueryHandler> _logger;

    /// <summary>
    /// Creates the room tenant-join QR handler.
    /// </summary>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="partyService">The current landlord service.</param>
    /// <param name="logger">The logger used for query completion tracking.</param>
    public GetRoomTenantJoinQrQueryHandler(
        ITenantService tenantService,
        IPartyService partyService,
        ILogger<GetRoomTenantJoinQrQueryHandler> logger)
    {
        _tenantService = tenantService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Generates a tenant join QR token for a landlord-managed room.
    /// </summary>
    /// <param name="request">The room tenant-join QR query.</param>
    /// <param name="cancellationToken">The token used to cancel token generation.</param>
    /// <returns>The QR token response.</returns>
    public async ValueTask<ResponseDto<TenantJoinQrResponseDto>> Handle(
        GetRoomTenantJoinQrQuery request,
        CancellationToken cancellationToken)
    {
        // QR generation is landlord-scoped so guests cannot mint join links for arbitrary rooms.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var response = await _tenantService.CreateTenantJoinQrAsync(
            new TenantJoinQrRequestModel
            {
                CurrentParty = currentParty,
                RoomPublicId = request.RoomPublicId,
                RoleCode = request.RoleCode
            },
            cancellationToken);

        // The response contains only the short-lived token; FE chooses how to render the QR.
        _logger.LogInformation(
            ApplicationLogConstants.TenantLogs.TENANT_JOIN_QR_QUERY_COMPLETED,
            request.RoomPublicId,
            currentParty.PartyPublicId);

        return new ResponseDto<TenantJoinQrResponseDto>(response);
    }
}
