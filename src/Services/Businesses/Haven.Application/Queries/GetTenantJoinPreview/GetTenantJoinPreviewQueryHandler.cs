namespace Haven.Application.Queries.GetTenantJoinPreview;

/// <summary>
/// Coordinates room join preview reads from short-lived QR tokens.
/// </summary>
public class GetTenantJoinPreviewQueryHandler
    : IQueryHandler<GetTenantJoinPreviewQuery, ResponseDto<TenantJoinPreviewResponseDto>>
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<GetTenantJoinPreviewQueryHandler> _logger;

    /// <summary>
    /// Creates the tenant join preview handler.
    /// </summary>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="logger">The logger used for query completion tracking.</param>
    public GetTenantJoinPreviewQueryHandler(
        ITenantService tenantService,
        ILogger<GetTenantJoinPreviewQueryHandler> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Loads the room preview encoded by a tenant join token.
    /// </summary>
    /// <param name="request">The tenant join preview query.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The tenant join preview response.</returns>
    public async ValueTask<ResponseDto<TenantJoinPreviewResponseDto>> Handle(
        GetTenantJoinPreviewQuery request,
        CancellationToken cancellationToken)
    {
        // Preview is token-scoped; the service resolves cached payload and room access data.
        var response = await _tenantService.GetTenantJoinPreviewAsync(
            new TenantJoinPreviewRequestModel { Token = request.Token },
            cancellationToken);

        // Return only preview data so the tenant must still confirm before joining the room.
        _logger.LogInformation(ApplicationLogConstants.TenantLogs.TENANT_JOIN_PREVIEW_QUERY_COMPLETED);

        return new ResponseDto<TenantJoinPreviewResponseDto>(response);
    }
}
