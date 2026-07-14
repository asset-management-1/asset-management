namespace Haven.Application.Commands.ConfirmTenantJoin;

/// <summary>
/// Coordinates tenant QR join confirmation after a room invite is scanned.
/// </summary>
public class ConfirmTenantJoinCommandHandler : ICommandHandler<ConfirmTenantJoinCommand, ResponseDto<TenantDetailResponseDto>>
{
    private readonly ITenantService _tenantService;
    private readonly IPartyService _partyService;
    private readonly ILogger<ConfirmTenantJoinCommandHandler> _logger;

    /// <summary>
    /// Creates the tenant join confirm handler.
    /// </summary>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="partyService">The current party service.</param>
    /// <param name="logger">The logger used for command completion tracking.</param>
    public ConfirmTenantJoinCommandHandler(
        ITenantService tenantService,
        IPartyService partyService,
        ILogger<ConfirmTenantJoinCommandHandler> logger)
    {
        _tenantService = tenantService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Confirms a tenant QR join token for the current party.
    /// </summary>
    /// <param name="request">The tenant join confirmation command.</param>
    /// <param name="cancellationToken">The token used to cancel the pending room join.</param>
    /// <returns>The joined tenant detail response.</returns>
    public async ValueTask<ResponseDto<TenantDetailResponseDto>> Handle(
        ConfirmTenantJoinCommand request,
        CancellationToken cancellationToken)
    {
        // Resolve party context first because a user may hold multiple business roles.
        var currentParty = await _partyService.GetCurrentPartyAsync(cancellationToken);
        var serviceRequest = request.Adapt<TenantJoinConfirmRequestModel>();
        serviceRequest.CurrentParty = currentParty;

        // The service validates the QR payload and applies the role stored in the token.
        var response = await _tenantService.ConfirmTenantJoinAsync(serviceRequest, cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.TenantLogs.TENANT_JOIN_CONFIRM_COMMAND_COMPLETED, response.Id);

        return new ResponseDto<TenantDetailResponseDto>(response);
    }
}
