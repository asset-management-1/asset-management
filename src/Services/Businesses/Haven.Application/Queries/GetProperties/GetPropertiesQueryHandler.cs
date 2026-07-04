namespace Haven.Application.Queries.GetProperties;

/// <summary>
/// Handles property list reads for the landlord building screen.
/// </summary>
public class GetPropertiesQueryHandler : IQueryHandler<GetPropertiesQuery, ResponseDto<PaginationResponse<IReadOnlyList<PropertyListItemResponseDto>>>>
{
    private readonly IPropertyService _propertyService;
    private readonly IPartyService _partyService;
    private readonly ILogger<GetPropertiesQueryHandler> _logger;

    /// <summary>
    /// Creates the property list query handler.
    /// </summary>
    /// <param name="propertyService">The service that loads property cards.</param>
    /// <param name="partyService">The service that resolves the current party context.</param>
    /// <param name="logger">The logger used for query completion tracking.</param>
    public GetPropertiesQueryHandler(
        IPropertyService propertyService,
        IPartyService partyService,
        ILogger<GetPropertiesQueryHandler> logger)
    {
        _propertyService = propertyService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Loads the current landlord's property list.
    /// </summary>
    /// <param name="request">The property list query.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The standardized paged property response.</returns>
    public async ValueTask<ResponseDto<PaginationResponse<IReadOnlyList<PropertyListItemResponseDto>>>> Handle(
        GetPropertiesQuery request,
        CancellationToken cancellationToken)
    {
        // Application boundary resolves party scope before mapping the read request.
        var currentParty = await _partyService.GetCurrentPartyAsync(cancellationToken);
        var propertyRequest = request.Adapt<PropertyListRequestModel>();
        propertyRequest.CurrentParty = currentParty;

        // Infrastructure service owns repository reads and nested response assembly.
        var response = await _propertyService.GetPropertiesAsync(propertyRequest, cancellationToken);
        _logger.LogInformation(LOG_PROPERTY_LIST_QUERY_COMPLETED, currentParty.PartyPublicId);

        return new ResponseDto<PaginationResponse<IReadOnlyList<PropertyListItemResponseDto>>>(response);
    }
}
