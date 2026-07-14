namespace Haven.Api.Controllers.v1;

/// <summary>
/// Exposes vehicle registration APIs inside one landlord-managed room.
/// </summary>
[ApiVersion(API_VERSION_1)]
[Route(ROOM_VEHICLES_ROOT)]
public sealed class RoomVehiclesController : BaseApiController
{
    /// <summary>
    /// Returns every active vehicle registered to one landlord-managed room.
    /// </summary>
    /// <param name="roomId">The room identifier from the route.</param>
    /// <returns>The standardized room vehicle collection.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<VehicleListItemResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVehicles(
        [FromRoute(Name = ROOM_ID_ROUTE_PARAMETER)] Guid roomId)
    {
        return Ok(await Mediator.Send(new GetVehiclesQuery(roomId)));
    }

    /// <summary>
    /// Registers one room vehicle using the vehicle registration form.
    /// </summary>
    /// <param name="roomId">The room identifier from the route.</param>
    /// <param name="form">The multipart vehicle registration form.</param>
    /// <returns>The created vehicle detail response.</returns>
    [HttpPost]
    [Consumes(MULTIPART_FORM_DATA)]
    [RequestSizeLimit(Be.Haven.Shared.Constants.ObjectStorageConstants.MAX_VEHICLE_MULTIPART_BODY_BYTES)]
    [ProducesResponseType(typeof(ResponseDto<VehicleDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateVehicle(
        [FromRoute(Name = ROOM_ID_ROUTE_PARAMETER)] Guid roomId,
        [FromForm] CreateRoomVehicleForm form)
    {
        var command = form.Adapt<CreateVehicleCommand>();
        command.RoomId = roomId;

        return Ok(await Mediator.Send(command));
    }

    /// <summary>
    /// Returns one vehicle registration detail.
    /// </summary>
    /// <param name="roomId">The room identifier from the route.</param>
    /// <param name="vehicleId">The vehicle identifier from the route.</param>
    /// <returns>The vehicle detail response.</returns>
    [HttpGet]
    [Route(ROOM_VEHICLE_BY_ID)]
    [ProducesResponseType(typeof(ResponseDto<VehicleDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVehicleDetail(
        [FromRoute(Name = ROOM_ID_ROUTE_PARAMETER)] Guid roomId,
        [FromRoute(Name = VEHICLE_ID_ROUTE_PARAMETER)] Guid vehicleId)
    {
        return Ok(await Mediator.Send(new GetVehicleDetailQuery(roomId, vehicleId)));
    }

    /// <summary>
    /// Updates the submitted fields of one vehicle registration.
    /// </summary>
    /// <param name="roomId">The room identifier from the route.</param>
    /// <param name="vehicleId">The vehicle identifier from the route.</param>
    /// <param name="form">The multipart partial-update form.</param>
    /// <returns>The updated vehicle detail response.</returns>
    [HttpPut]
    [Route(ROOM_VEHICLE_BY_ID)]
    [Consumes(MULTIPART_FORM_DATA)]
    [RequestSizeLimit(Be.Haven.Shared.Constants.ObjectStorageConstants.MAX_VEHICLE_MULTIPART_BODY_BYTES)]
    [ProducesResponseType(typeof(ResponseDto<VehicleDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVehicle(
        [FromRoute(Name = ROOM_ID_ROUTE_PARAMETER)] Guid roomId,
        [FromRoute(Name = VEHICLE_ID_ROUTE_PARAMETER)] Guid vehicleId,
        [FromForm] UpdateRoomVehicleForm form)
    {
        var command = form.Adapt<UpdateVehicleCommand>();
        command.RoomId = roomId;
        command.VehicleId = vehicleId;

        return Ok(await Mediator.Send(command));
    }

    /// <summary>
    /// Soft-deletes one vehicle registration.
    /// </summary>
    /// <param name="roomId">The room identifier from the route.</param>
    /// <param name="vehicleId">The vehicle identifier from the route.</param>
    /// <returns>The successful operation response.</returns>
    [HttpDelete]
    [Route(ROOM_VEHICLE_BY_ID)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVehicle(
        [FromRoute(Name = ROOM_ID_ROUTE_PARAMETER)] Guid roomId,
        [FromRoute(Name = VEHICLE_ID_ROUTE_PARAMETER)] Guid vehicleId)
    {
        return Ok(await Mediator.Send(new DeleteVehicleCommand(roomId, vehicleId)));
    }
}
