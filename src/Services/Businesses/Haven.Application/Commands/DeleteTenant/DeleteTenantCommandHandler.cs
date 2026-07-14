namespace Haven.Application.Commands.DeleteTenant;

/// <summary>
/// Coordinates landlord-scoped tenant move-out requests.
/// </summary>
public class DeleteTenantCommandHandler : ICommandHandler<DeleteTenantCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly ITenantService _tenantService;
    private readonly IPartyService _partyService;
    private readonly ILogger<DeleteTenantCommandHandler> _logger;

    /// <summary>
    /// Creates the tenant delete handler.
    /// </summary>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="partyService">The current landlord service.</param>
    /// <param name="logger">The logger used for command completion tracking.</param>
    public DeleteTenantCommandHandler(
        ITenantService tenantService,
        IPartyService partyService,
        ILogger<DeleteTenantCommandHandler> logger)
    {
        _tenantService = tenantService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Moves a tenant out of a landlord-managed room.
    /// </summary>
    /// <param name="request">The tenant delete command.</param>
    /// <param name="cancellationToken">The token used to cancel the tenant move-out.</param>
    /// <returns>The move-out operation status.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        DeleteTenantCommand request,
        CancellationToken cancellationToken)
    {
        // Move-out is landlord-scoped and must not delete tenant identity data.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var response = await _tenantService.DeleteTenantAsync(
            new TenantDeleteRequestModel
            {
                CurrentParty = currentParty,
                OccupancyPublicId = request.Id
            },
            cancellationToken);

        // Return a simple operation envelope; cleanup details stay inside the service transaction.
        _logger.LogInformation(
            ApplicationLogConstants.TenantLogs.TENANT_DELETE_COMMAND_COMPLETED,
            request.Id,
            currentParty.PartyPublicId);

        return new ResponseDto<OperationStatusResponseDto>(response);
    }
}
