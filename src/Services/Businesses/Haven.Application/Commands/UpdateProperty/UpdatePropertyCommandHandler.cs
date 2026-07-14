namespace Haven.Application.Commands.UpdateProperty;

/// <summary>
/// Handles landlord property update requests.
/// </summary>
public class UpdatePropertyCommandHandler : ICommandHandler<UpdatePropertyCommand, ResponseDto<PropertyDetailResponseDto>>
{
    private readonly IPropertyService _propertyService;
    private readonly IPartyService _partyService;
    private readonly ILogger<UpdatePropertyCommandHandler> _logger;

    /// <summary>
    /// Creates the property update handler.
    /// </summary>
    /// <param name="propertyService">The service that updates the property graph transactionally.</param>
    /// <param name="partyService">The service that resolves the current landlord party.</param>
    /// <param name="logger">The logger used for command completion tracking.</param>
    public UpdatePropertyCommandHandler(
        IPropertyService propertyService,
        IPartyService partyService,
        ILogger<UpdatePropertyCommandHandler> logger)
    {
        _propertyService = propertyService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Updates a property and returns the refreshed detail response.
    /// </summary>
    /// <param name="request">The update property command.</param>
    /// <param name="cancellationToken">The token used to cancel the property mutation.</param>
    /// <returns>The standardized refreshed property detail response.</returns>
    public async ValueTask<ResponseDto<PropertyDetailResponseDto>> Handle(
        UpdatePropertyCommand request,
        CancellationToken cancellationToken)
    {
        // Application boundary resolves landlord context before shaping the update request.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var propertyRequest = request.Adapt<PropertyUpdateRequestModel>();
        propertyRequest.CurrentParty = currentParty;

        // The infrastructure service owns lookup validation, safe replace, and transaction persistence.
        var response = await _propertyService.UpdatePropertyAsync(propertyRequest, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.PropertyLogs.PROPERTY_UPDATE_COMMAND_COMPLETED,
            request.Id,
            currentParty.PartyPublicId);

        return new ResponseDto<PropertyDetailResponseDto>(response);
    }
}
