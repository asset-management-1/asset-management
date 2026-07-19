namespace Haven.Api.Controllers.v1;

/// <summary>
/// Exposes standalone room package APIs for landlord room management.
/// </summary>
[ApiVersion(API_VERSION_1)]
public sealed class RoomPackagesController : BaseApiController
{
    /// <summary>
    /// Returns the effective package set for one room.
    /// </summary>
    /// <param name="request">The room package query containing the frontend-safe room identifier.</param>
    /// <returns>The effective room package list.</returns>
    [HttpGet]
    [SwaggerValueExample<RoomPackageSwaggerExamples.Values>]
    [SwaggerResponseExample<RoomPackageSwaggerExamples.ListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<RoomPackageListResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoomPackages([FromQuery] GetRoomPackagesQuery request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Returns one package from a room's effective package set.
    /// </summary>
    /// <param name="id">The frontend-safe package identifier.</param>
    /// <param name="roomId">The frontend-safe room identifier.</param>
    /// <returns>The requested room package detail.</returns>
    [HttpGet]
    [Route(BY_ID)]
    [SwaggerValueExample<RoomPackageSwaggerExamples.Values>]
    [SwaggerResponseExample<RoomPackageSwaggerExamples.DetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<RoomPackageDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoomPackageDetail(
        [FromRoute(Name = ID_ROUTE_PARAMETER)] Guid id,
        [FromQuery] Guid roomId)
    {
        return Ok(await Mediator.Send(new GetRoomPackageDetailQuery(id, roomId)));
    }

    /// <summary>
    /// Creates one room-owned package.
    /// </summary>
    /// <param name="request">The room identifier and package fields.</param>
    /// <returns>The created room package detail.</returns>
    [HttpPost]
    [SwaggerRequestExample<RoomPackageSwaggerExamples.CreateRequest>]
    [SwaggerResponseExample<RoomPackageSwaggerExamples.DetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<RoomPackageDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRoomPackage([FromBody] CreateRoomPackageCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Applies submitted fields to one room package.
    /// </summary>
    /// <param name="request">The package identifier, room identifier, and partial fields.</param>
    /// <returns>The updated room package detail.</returns>
    [HttpPut]
    [SwaggerRequestExample<RoomPackageSwaggerExamples.UpdateRequest>]
    [SwaggerResponseExample<RoomPackageSwaggerExamples.DetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<RoomPackageDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRoomPackage([FromBody] UpdateRoomPackageCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Soft-deletes one editable room package.
    /// </summary>
    /// <param name="id">The frontend-safe package identifier.</param>
    /// <param name="roomId">The frontend-safe room identifier.</param>
    /// <returns>The successful operation response.</returns>
    [HttpDelete]
    [Route(BY_ID)]
    [SwaggerValueExample<RoomPackageSwaggerExamples.Values>]
    [SwaggerResponseExample<RoomPackageSwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRoomPackage(
        [FromRoute(Name = ID_ROUTE_PARAMETER)] Guid id,
        [FromQuery] Guid roomId)
    {
        return Ok(await Mediator.Send(new DeleteRoomPackageCommand(id, roomId)));
    }
}
