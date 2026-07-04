namespace Haven.Application.Queries.GetPropertyDetail;

/// <summary>
/// Handles property detail reads for the landlord building detail screen.
/// </summary>
public class GetPropertyDetailQueryHandler : IQueryHandler<GetPropertyDetailQuery, ResponseDto<PropertyDetailResponseDto>>
{
    private readonly IPropertyService _propertyService;
    private readonly IPartyService _partyService;
    private readonly ILogger<GetPropertyDetailQueryHandler> _logger;

    /// <summary>
    /// Creates the property detail query handler.
    /// </summary>
    /// <param name="propertyService">The service that loads property detail.</param>
    /// <param name="partyService">The service that resolves the current party context.</param>
    /// <param name="logger">The logger used for query completion tracking.</param>
    public GetPropertyDetailQueryHandler(
        IPropertyService propertyService,
        IPartyService partyService,
        ILogger<GetPropertyDetailQueryHandler> logger)
    {
        _propertyService = propertyService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Loads a property detail response by public identifier.
    /// </summary>
    /// <param name="request">The property detail query.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The standardized property detail response.</returns>
    public async ValueTask<ResponseDto<PropertyDetailResponseDto>> Handle(
        GetPropertyDetailQuery request,
        CancellationToken cancellationToken)
    {
        // Application boundary resolves party scope before mapping the detail request.
        var currentParty = await _partyService.GetCurrentPartyAsync(cancellationToken);
        var propertyRequest = request.Adapt<PropertyDetailRequestModel>();
        propertyRequest.CurrentParty = currentParty;

        // Infrastructure service hides missing and unauthorized properties through the same not-found path.
        var response = await _propertyService.GetPropertyDetailAsync(propertyRequest, cancellationToken);
        _logger.LogInformation(
            LOG_PROPERTY_DETAIL_QUERY_COMPLETED,
            request.PropertyPublicId,
            currentParty.PartyPublicId);

        return new ResponseDto<PropertyDetailResponseDto>(response);
    }
}
