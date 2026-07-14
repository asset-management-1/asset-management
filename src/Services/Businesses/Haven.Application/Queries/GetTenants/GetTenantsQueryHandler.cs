namespace Haven.Application.Queries.GetTenants;

/// <summary>
/// Coordinates landlord-scoped tenant list reads.
/// </summary>
public class GetTenantsQueryHandler
    : IQueryHandler<GetTenantsQuery, ResponseDto<PaginationResponse<IReadOnlyList<TenantListItemResponseDto>>>>
{
    private readonly ITenantService _tenantService;
    private readonly IPartyService _partyService;
    private readonly ILogger<GetTenantsQueryHandler> _logger;

    /// <summary>
    /// Creates the tenant list handler.
    /// </summary>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="partyService">The current party service.</param>
    /// <param name="logger">The logger used for query completion tracking.</param>
    public GetTenantsQueryHandler(
        ITenantService tenantService,
        IPartyService partyService,
        ILogger<GetTenantsQueryHandler> logger)
    {
        _tenantService = tenantService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Loads tenants visible to the current party.
    /// </summary>
    /// <param name="request">The tenant list query.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The paged tenant list response.</returns>
    public async ValueTask<ResponseDto<PaginationResponse<IReadOnlyList<TenantListItemResponseDto>>>> Handle(
        GetTenantsQuery request,
        CancellationToken cancellationToken)
    {
        // Resolve current party before mapping so the service receives one scoped request model.
        var currentParty = await _partyService.GetCurrentPartyAsync(cancellationToken);
        var serviceRequest = request.Adapt<TenantListRequestModel>();
        serviceRequest.CurrentParty = currentParty;

        // Delegate list composition to the tenant service and return the standard API envelope.
        var response = await _tenantService.GetTenantsAsync(serviceRequest, cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.TenantLogs.TENANT_LIST_QUERY_COMPLETED, currentParty.PartyPublicId);

        return new ResponseDto<PaginationResponse<IReadOnlyList<TenantListItemResponseDto>>>(response);
    }
}
