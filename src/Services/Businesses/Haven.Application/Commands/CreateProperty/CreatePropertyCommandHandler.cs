namespace Haven.Application.Commands.CreateProperty;

/// <summary>
/// Handles landlord property creation.
/// </summary>
public class CreatePropertyCommandHandler : ICommandHandler<CreatePropertyCommand, ResponseDto<CreatedPropertyResponseDto>>
{
    private readonly IPropertyService _propertyService;
    private readonly IPartyService _partyService;
    private readonly ILogger<CreatePropertyCommandHandler> _logger;

    /// <summary>
    /// Creates the property creation handler.
    /// </summary>
    /// <param name="propertyService">The service that creates properties transactionally.</param>
    /// <param name="partyService">The service that resolves the current party context.</param>
    /// <param name="logger">The logger used for command completion tracking.</param>
    public CreatePropertyCommandHandler(
        IPropertyService propertyService,
        IPartyService partyService,
        ILogger<CreatePropertyCommandHandler> logger)
    {
        _propertyService = propertyService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a property from the final floor and room structure submitted by the UI.
    /// </summary>
    /// <param name="request">The create property command.</param>
    /// <param name="cancellationToken">The token used to cancel property creation.</param>
    /// <returns>The standardized created-property response.</returns>
    public async ValueTask<ResponseDto<CreatedPropertyResponseDto>> Handle(
        CreatePropertyCommand request,
        CancellationToken cancellationToken)
    {
        // Application boundary resolves the current landlord before shaping the service request.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var propertyRequest = request.Adapt<PropertyCreationRequestModel>();
        propertyRequest.CurrentParty = currentParty;

        // Infrastructure service owns master-data/location resolution and transactional persistence.
        var response = await _propertyService.CreatePropertyAsync(propertyRequest, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.PropertyLogs.PROPERTY_CREATE_COMMAND_COMPLETED,
            response.Id,
            currentParty.PartyPublicId);

        return new ResponseDto<CreatedPropertyResponseDto>(response);
    }
}
