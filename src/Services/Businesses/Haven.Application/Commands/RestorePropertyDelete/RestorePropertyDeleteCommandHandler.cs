namespace Haven.Application.Commands.RestorePropertyDelete;

/// <summary>
/// Handles landlord property pending-delete restore requests.
/// </summary>
public class RestorePropertyDeleteCommandHandler : ICommandHandler<RestorePropertyDeleteCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IPropertyService _propertyService;
    private readonly IPartyService _partyService;
    private readonly ILogger<RestorePropertyDeleteCommandHandler> _logger;

    /// <summary>
    /// Creates the property restore-delete handler.
    /// </summary>
    /// <param name="propertyService">The service that restores pending property delete requests.</param>
    /// <param name="partyService">The service that resolves the current landlord party.</param>
    /// <param name="logger">The logger used for command completion tracking.</param>
    public RestorePropertyDeleteCommandHandler(
        IPropertyService propertyService,
        IPartyService partyService,
        ILogger<RestorePropertyDeleteCommandHandler> logger)
    {
        _propertyService = propertyService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Restores a pending property delete request.
    /// </summary>
    /// <param name="request">The restore delete command.</param>
    /// <param name="cancellationToken">The token used to cancel delete restoration.</param>
    /// <returns>The standardized operation status response.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        RestorePropertyDeleteCommand request,
        CancellationToken cancellationToken)
    {
        // Application boundary resolves landlord context before the service checks ownership.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var propertyRequest = new PropertyDeleteRequestModel
        {
            CurrentParty = currentParty,
            PropertyPublicId = request.PropertyPublicId
        };

        // Clear the scheduled deletion only after the service confirms the property belongs to this landlord scope.
        var response = await _propertyService.RestorePropertyDeleteAsync(propertyRequest, cancellationToken);

        // Record the completed restore with public identifiers only.
        _logger.LogInformation(
            ApplicationLogConstants.PropertyLogs.PROPERTY_RESTORE_DELETE_COMMAND_COMPLETED,
            request.PropertyPublicId,
            currentParty.PartyPublicId);

        return new ResponseDto<OperationStatusResponseDto>(response);
    }
}
