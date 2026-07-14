namespace Haven.Application.Commands.CreateTenant;

/// <summary>
/// Coordinates landlord-driven tenant or occupant creation for a room.
/// </summary>
public class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, ResponseDto<TenantDetailResponseDto>>
{
    private readonly ITenantService _tenantService;
    private readonly IPartyService _partyService;
    private readonly ILogger<CreateTenantCommandHandler> _logger;

    /// <summary>
    /// Creates the tenant create handler.
    /// </summary>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="partyService">The current landlord service.</param>
    /// <param name="logger">The logger used for command completion tracking.</param>
    public CreateTenantCommandHandler(
        ITenantService tenantService,
        IPartyService partyService,
        ILogger<CreateTenantCommandHandler> logger)
    {
        _tenantService = tenantService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Adds a primary tenant or occupant to a room under the current landlord.
    /// </summary>
    /// <param name="request">The tenant creation command.</param>
    /// <param name="cancellationToken">The token used to cancel tenant creation.</param>
    /// <returns>The created tenant detail response.</returns>
    public async ValueTask<ResponseDto<TenantDetailResponseDto>> Handle(
        CreateTenantCommand request,
        CancellationToken cancellationToken)
    {
        // Tenant creation is a landlord action, so resolve landlord context before service mapping.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var serviceRequest = request.Adapt<TenantCreateRequestModel>();
        serviceRequest.CurrentParty = currentParty;

        // The service owns tenant source resolution, contract creation, and occupancy persistence.
        var response = await _tenantService.CreateTenantAsync(serviceRequest, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.TenantLogs.TENANT_CREATE_COMMAND_COMPLETED,
            response.Id,
            currentParty.PartyPublicId);

        return new ResponseDto<TenantDetailResponseDto>(response);
    }
}
