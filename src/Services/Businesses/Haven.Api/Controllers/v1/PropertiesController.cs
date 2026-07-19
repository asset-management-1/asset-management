namespace Haven.Api.Controllers.v1;

/// <summary>
/// Exposes landlord property/building APIs for the Haven property management UI.
/// </summary>
[ApiVersion(API_VERSION_1)]
public class PropertiesController : BaseApiController
{
    /// <summary>
    /// Returns the current landlord's paged property list with optional floor and room cards.
    /// </summary>
    /// <param name="request">The property list filters.</param>
    /// <returns>The standardized paged property list response.</returns>
    [HttpGet]
    [SwaggerValueExample<PropertySwaggerExamples.ListValues>]
    [SwaggerResponseExample<PropertySwaggerExamples.ListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<PaginationResponse<IReadOnlyList<PropertyListItemResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProperties([FromQuery] GetPropertiesQuery request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Returns one landlord-scoped property/building detail.
    /// </summary>
    /// <param name="propertyPublicId">The public property identifier from the route.</param>
    /// <returns>The standardized property detail response.</returns>
    [HttpGet]
    [Route(PROPERTY_BY_PUBLIC_ID)]
    [SwaggerValueExample<PropertySwaggerExamples.RouteValues>]
    [SwaggerResponseExample<PropertySwaggerExamples.DetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<PropertyDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPropertyDetail(
        [FromRoute(Name = PROPERTY_PUBLIC_ID_ROUTE_PARAMETER)] Guid propertyPublicId)
    {
        return Ok(await Mediator.Send(new GetPropertyDetailQuery(propertyPublicId)));
    }

    /// <summary>
    /// Creates a property/building from the submitted floor and room structure.
    /// </summary>
    /// <param name="request">The create property request.</param>
    /// <returns>The standardized created-property response.</returns>
    [HttpPost]
    [SwaggerRequestExample<PropertySwaggerExamples.CreateRequest>]
    [SwaggerResponseExample<PropertySwaggerExamples.CreateResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<CreatedPropertyResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Updates one landlord-scoped property/building edit form.
    /// </summary>
    /// <param name="request">The update property request body, including the frontend-safe property identifier.</param>
    /// <returns>The standardized refreshed property detail response.</returns>
    [HttpPut]
    [SwaggerRequestExample<PropertySwaggerExamples.UpdateRequest>]
    [SwaggerResponseExample<PropertySwaggerExamples.DetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<PropertyDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProperty([FromBody] UpdatePropertyCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Soft-deletes one landlord-scoped property/building when no protected dependency data exists.
    /// </summary>
    /// <param name="propertyPublicId">The public property identifier from the route.</param>
    /// <returns>The standardized delete status response.</returns>
    [HttpDelete]
    [Route(PROPERTY_BY_PUBLIC_ID)]
    [SwaggerValueExample<PropertySwaggerExamples.RouteValues>]
    [SwaggerResponseExample<PropertySwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProperty(
        [FromRoute(Name = PROPERTY_PUBLIC_ID_ROUTE_PARAMETER)] Guid propertyPublicId)
    {
        return Ok(await Mediator.Send(new DeletePropertyCommand(propertyPublicId)));
    }

    /// <summary>
    /// Restores one landlord-scoped property/building pending-delete request.
    /// </summary>
    /// <param name="propertyPublicId">The public property identifier from the route.</param>
    /// <returns>The standardized restore status response.</returns>
    [HttpPost]
    [Route(PROPERTY_RESTORE_DELETE)]
    [SwaggerValueExample<PropertySwaggerExamples.RouteValues>]
    [SwaggerResponseExample<PropertySwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RestorePropertyDelete(
        [FromRoute(Name = PROPERTY_PUBLIC_ID_ROUTE_PARAMETER)] Guid propertyPublicId)
    {
        return Ok(await Mediator.Send(new RestorePropertyDeleteCommand(propertyPublicId)));
    }
}
