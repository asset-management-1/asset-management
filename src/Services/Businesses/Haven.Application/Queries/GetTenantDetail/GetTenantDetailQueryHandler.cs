namespace Haven.Application.Queries.GetTenantDetail;

/// <summary>
/// Coordinates tenant occupancy detail reads under the current party scope.
/// </summary>
public class GetTenantDetailQueryHandler : IQueryHandler<GetTenantDetailQuery, ResponseDto<TenantDetailResponseDto>>
{
    private readonly ITenantService _tenantService;
    private readonly IPartyService _partyService;
    private readonly ILogger<GetTenantDetailQueryHandler> _logger;

    /// <summary>
    /// Creates the tenant detail handler.
    /// </summary>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="partyService">The current party service.</param>
    /// <param name="logger">The logger used for query completion tracking.</param>
    public GetTenantDetailQueryHandler(
        ITenantService tenantService,
        IPartyService partyService,
        ILogger<GetTenantDetailQueryHandler> logger)
    {
        _tenantService = tenantService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Loads one tenant occupancy detail under the current party.
    /// </summary>
    /// <param name="request">The tenant detail query.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The tenant detail response.</returns>
    public async ValueTask<ResponseDto<TenantDetailResponseDto>> Handle(
        GetTenantDetailQuery request,
        CancellationToken cancellationToken)
    {
        // Resolve current party before the service enforces occupancy visibility.
        var currentParty = await _partyService.GetCurrentPartyAsync(cancellationToken);
        var response = await _tenantService.GetTenantDetailAsync(
            new TenantDetailRequestModel
            {
                CurrentParty = currentParty,
                OccupancyPublicId = request.Id
            },
            cancellationToken);

        // Keep the API envelope thin; detail composition belongs to the tenant service.
        _logger.LogInformation(
            ApplicationLogConstants.TenantLogs.TENANT_DETAIL_QUERY_COMPLETED,
            request.Id,
            currentParty.PartyPublicId);

        return new ResponseDto<TenantDetailResponseDto>(response);
    }
}
