namespace Haven.Api.Controllers.v1;

/// <summary>
/// Exposes monthly electricity and water meter APIs for one landlord-managed room.
/// </summary>
[ApiVersion(API_VERSION_1)]
[Route(ROOM_METERS_ROOT)]
public sealed class RoomMetersController : BaseApiController
{
    /// <summary>
    /// Loads at most twelve monthly meter periods for one room and calendar year.
    /// </summary>
    /// <param name="roomId">The room identifier from the route.</param>
    /// <param name="year">The calendar year selected by the user.</param>
    /// <returns>The room meter history ordered from newest to oldest.</returns>
    [HttpGet]
    public async Task<IActionResult> GetHistory(
        [FromRoute(Name = ROOM_ID_ROUTE_PARAMETER)] Guid roomId,
        [FromQuery] int year)
    {
        return Ok(await Mediator.Send(new GetRoomMeterHistoryQuery(roomId, year)));
    }

    /// <summary>
    /// Loads one meter month for display or mutation prefill.
    /// </summary>
    /// <param name="roomId">The room identifier from the route.</param>
    /// <param name="month">The selected calendar month.</param>
    /// <param name="year">The selected calendar year.</param>
    /// <returns>The current meter period and its live invoice impact.</returns>
    [HttpGet]
    [Route(ROOM_METERS_PERIOD)]
    public async Task<IActionResult> GetPeriod(
        [FromRoute(Name = ROOM_ID_ROUTE_PARAMETER)] Guid roomId,
        [FromQuery] int month,
        [FromQuery] int year)
    {
        return Ok(await Mediator.Send(new GetRoomMeterPeriodQuery(roomId, month, year)));
    }

    /// <summary>
    /// Creates and confirms one complete room meter month.
    /// </summary>
    /// <param name="roomId">The room identifier from the route.</param>
    /// <param name="form">The multipart meter values, prices, and optional evidence images.</param>
    /// <returns>The committed meter period.</returns>
    [HttpPost]
    [Consumes(MULTIPART_FORM_DATA)]
    [RequestSizeLimit(Be.Haven.Shared.Constants.ObjectStorageConstants.MAX_METER_MULTIPART_BODY_BYTES)]
    [ProducesResponseType(typeof(ResponseDto<MeterPeriodDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromRoute(Name = ROOM_ID_ROUTE_PARAMETER)] Guid roomId,
        [FromForm] RoomMeterForm form)
    {
        var command = form.Adapt<CreateMeterCommand>();
        command.RoomId = roomId;

        return Ok(await Mediator.Send(command));
    }

    /// <summary>
    /// Updates and reconfirms one existing room meter month.
    /// </summary>
    /// <param name="roomId">The room identifier from the route.</param>
    /// <param name="form">The multipart meter values, prices, selected image additions, and selected image removals.</param>
    /// <returns>The committed meter period.</returns>
    [HttpPut]
    [Consumes(MULTIPART_FORM_DATA)]
    [RequestSizeLimit(Be.Haven.Shared.Constants.ObjectStorageConstants.MAX_METER_MULTIPART_BODY_BYTES)]
    [ProducesResponseType(typeof(ResponseDto<MeterPeriodDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        [FromRoute(Name = ROOM_ID_ROUTE_PARAMETER)] Guid roomId,
        [FromForm] UpdateRoomMeterForm form)
    {
        var command = form.Adapt<UpdateMeterCommand>();
        command.RoomId = roomId;

        return Ok(await Mediator.Send(command));
    }
}
