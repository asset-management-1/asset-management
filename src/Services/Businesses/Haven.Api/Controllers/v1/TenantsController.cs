namespace Haven.Api.Controllers.v1;

/// <summary>
/// Exposes landlord tenant management APIs.
/// </summary>
[ApiVersion(API_VERSION_1)]
public class TenantsController : BaseApiController
{
    /// <summary>
    /// Adds a tenant or member to a room.
    /// </summary>
    /// <param name="request">The tenant creation request.</param>
    /// <returns>The standardized tenant detail response.</returns>
    [HttpPost]
    [SwaggerRequestExample<TenantSwaggerExamples.CreateRequest>]
    [SwaggerResponseExample<TenantSwaggerExamples.DetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<TenantDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Returns tenant occupancies scoped to the current landlord party.
    /// </summary>
    /// <param name="request">The tenant-list query filters.</param>
    /// <returns>The standardized paged tenant response.</returns>
    [HttpGet]
    [SwaggerValueExample<TenantSwaggerExamples.Values>]
    [SwaggerResponseExample<TenantSwaggerExamples.ListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<PaginationResponse<IReadOnlyList<TenantListItemResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTenants([FromQuery] GetTenantsQuery request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Returns one tenant occupancy detail.
    /// </summary>
    /// <param name="id">The occupancy public identifier.</param>
    /// <returns>The standardized tenant detail response.</returns>
    [HttpGet]
    [Route(BY_ID)]
    [SwaggerValueExample<TenantSwaggerExamples.Values>]
    [SwaggerResponseExample<TenantSwaggerExamples.DetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<TenantDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTenantDetail([FromRoute(Name = ID_ROUTE_PARAMETER)] Guid id)
    {
        return Ok(await Mediator.Send(new GetTenantDetailQuery(id)));
    }

    /// <summary>
    /// Moves one tenant occupancy out of a room.
    /// </summary>
    /// <param name="id">The occupancy public identifier.</param>
    /// <returns>The standardized operation status response.</returns>
    [HttpDelete]
    [Route(BY_ID)]
    [SwaggerValueExample<TenantSwaggerExamples.Values>]
    [SwaggerResponseExample<TenantSwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTenant([FromRoute(Name = ID_ROUTE_PARAMETER)] Guid id)
    {
        return Ok(await Mediator.Send(new DeleteTenantCommand(id)));
    }

    /// <summary>
    /// Generates a room tenant-join QR token from the landlord room screen.
    /// </summary>
    /// <param name="roomPublicId">The room public identifier from the room route.</param>
    /// <param name="roleCode">The tenant role code encoded into the token.</param>
    /// <returns>The standardized QR token response.</returns>
    [HttpGet]
    [Route(ROOM_TENANT_JOIN_QR)]
    [SwaggerValueExample<TenantSwaggerExamples.Values>]
    [SwaggerResponseExample<TenantSwaggerExamples.JoinQrResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<TenantJoinQrResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRoomTenantJoinQr(
        [FromRoute(Name = ROOM_ID_ROUTE_PARAMETER)] Guid roomPublicId,
        string roleCode)
    {
        // Tenant join route ownership stays in this controller even though the public URL starts from a room.
        return Ok(await Mediator.Send(new GetRoomTenantJoinQrQuery(roomPublicId, roleCode)));
    }

    /// <summary>
    /// Returns room preview data for a tenant join token.
    /// </summary>
    /// <param name="token">The QR token from the scanned join link.</param>
    /// <returns>The standardized join preview response.</returns>
    [HttpGet]
    [Route(TENANT_JOIN_BY_TOKEN)]
    [SwaggerValueExample<TenantSwaggerExamples.Values>]
    [SwaggerResponseExample<TenantSwaggerExamples.JoinPreviewResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<TenantJoinPreviewResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTenantJoinPreview([FromRoute(Name = TENANT_JOIN_TOKEN_ROUTE_PARAMETER)] string token)
    {
        return Ok(await Mediator.Send(new GetTenantJoinPreviewQuery(token)));
    }

    /// <summary>
    /// Confirms a tenant join token for the current tenant party.
    /// </summary>
    /// <param name="request">The join token submitted by the scanning tenant.</param>
    /// <returns>The standardized tenant detail response.</returns>
    [HttpPost]
    [Route(TENANT_JOIN_ROOT)]
    [SwaggerRequestExample<TenantSwaggerExamples.ConfirmRequest>]
    [SwaggerResponseExample<TenantSwaggerExamples.DetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<TenantDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmTenantJoin([FromBody] ConfirmTenantJoinCommand request)
    {
        return Ok(await Mediator.Send(request));
    }
}
