namespace Haven.Application.Commands.DeleteVehicle;

/// <summary>
/// Coordinates landlord vehicle soft-delete requests.
/// </summary>
public class DeleteVehicleCommandHandler : ICommandHandler<DeleteVehicleCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IVehicleService _vehicleService;
    private readonly IPartyService _partyService;
    private readonly ILogger<DeleteVehicleCommandHandler> _logger;

    /// <summary>
    /// Creates the vehicle delete command handler.
    /// </summary>
    /// <param name="vehicleService">The vehicle workflow service.</param>
    /// <param name="partyService">The service used to resolve the current landlord.</param>
    /// <param name="logger">The structured command logger.</param>
    public DeleteVehicleCommandHandler(IVehicleService vehicleService, IPartyService partyService, ILogger<DeleteVehicleCommandHandler> logger)
    {
        _vehicleService = vehicleService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Soft-deletes a vehicle under the current landlord scope.
    /// </summary>
    /// <param name="request">The vehicle delete command.</param>
    /// <param name="cancellationToken">The token used to cancel the command.</param>
    /// <returns>The successful operation response envelope.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        // Resolve landlord scope before soft-deleting the requested room vehicle.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var response = await _vehicleService.DeleteAsync(new VehicleDeleteRequestModel
        {
            CurrentParty = currentParty,
            RoomPublicId = request.RoomId,
            VehiclePublicId = request.VehiclePublicId
        }, cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.VehicleLogs.VEHICLE_DELETE_COMMAND_COMPLETED, request.VehiclePublicId, currentParty.PartyPublicId);

        return new ResponseDto<OperationStatusResponseDto>(response);
    }
}
