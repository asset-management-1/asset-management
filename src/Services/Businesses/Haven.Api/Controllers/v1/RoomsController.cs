namespace Haven.Api.Controllers.v1;

/// <summary>
/// Exposes landlord room APIs for the Haven room management UI.
/// </summary>
[ApiVersion(API_VERSION_1)]
public class RoomsController : BaseApiController
{
    /// <summary>
    /// Returns grouped room cards for the current landlord party.
    /// </summary>
    /// <param name="request">The room-list query filters.</param>
    /// <returns>The standardized paged room response.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto<PaginationResponse<IReadOnlyList<RoomListPropertyResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRooms([FromQuery] GetRoomsQuery request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Returns one landlord-scoped room detail.
    /// </summary>
    /// <param name="roomPublicId">The public room identifier from the route.</param>
    /// <returns>The standardized room detail response.</returns>
    [HttpGet]
    [Route(ROOM_BY_ID)]
    [ProducesResponseType(typeof(ResponseDto<RoomDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRoomDetail(
        [FromRoute(Name = ROOM_ID_ROUTE_PARAMETER)] Guid roomPublicId)
    {
        return Ok(await Mediator.Send(new GetRoomDetailQuery(roomPublicId)));
    }

    /// <summary>
    /// Updates one room edit form.
    /// </summary>
    /// <param name="request">The room update request body, including the frontend-safe room identifier.</param>
    /// <returns>The standardized refreshed room detail response.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(ResponseDto<RoomDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRoom([FromBody] UpdateRoomCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Soft-deletes one room when no contract or active occupancy blocks it.
    /// </summary>
    /// <param name="roomPublicId">The public room identifier from the route.</param>
    /// <returns>The standardized delete status response.</returns>
    [HttpDelete]
    [Route(ROOM_BY_ID)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRoom(
        [FromRoute(Name = ROOM_ID_ROUTE_PARAMETER)] Guid roomPublicId)
    {
        return Ok(await Mediator.Send(new DeleteRoomCommand(roomPublicId)));
    }
}
