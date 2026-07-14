namespace Haven.Application.Commands.DeleteProperty;

/// <summary>
/// Handles landlord property soft-delete requests.
/// </summary>
public class DeletePropertyCommandHandler : ICommandHandler<DeletePropertyCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IPropertyService _propertyService;
    private readonly IPartyService _partyService;
    private readonly ILogger<DeletePropertyCommandHandler> _logger;

    /// <summary>
    /// Creates the property delete handler.
    /// </summary>
    /// <param name="propertyService">The service that soft-deletes properties safely.</param>
    /// <param name="partyService">The service that resolves the current landlord party.</param>
    /// <param name="logger">The logger used for command completion tracking.</param>
    public DeletePropertyCommandHandler(
        IPropertyService propertyService,
        IPartyService partyService,
        ILogger<DeletePropertyCommandHandler> logger)
    {
        _propertyService = propertyService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Soft-deletes a property when no protected dependencies exist.
    /// </summary>
    /// <param name="request">The delete property command.</param>
    /// <param name="cancellationToken">The token used to cancel delete scheduling.</param>
    /// <returns>The standardized operation status response.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        DeletePropertyCommand request,
        CancellationToken cancellationToken)
    {
        // Application boundary resolves landlord context before the service checks ownership and dependencies.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var propertyRequest = new PropertyDeleteRequestModel
        {
            CurrentParty = currentParty,
            PropertyPublicId = request.PropertyPublicId
        };

        // Delete is soft-delete only; dependency guards return the current business-error contract.
        var response = await _propertyService.DeletePropertyAsync(propertyRequest, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.PropertyLogs.PROPERTY_DELETE_COMMAND_COMPLETED,
            request.PropertyPublicId,
            currentParty.PartyPublicId);

        return new ResponseDto<OperationStatusResponseDto>(response);
    }
}
