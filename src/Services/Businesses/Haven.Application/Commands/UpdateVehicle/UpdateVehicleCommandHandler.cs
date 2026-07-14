namespace Haven.Application.Commands.UpdateVehicle;

/// <summary>
/// Coordinates partial landlord vehicle updates.
/// </summary>
public class UpdateVehicleCommandHandler : ICommandHandler<UpdateVehicleCommand, ResponseDto<VehicleDetailResponseDto>>
{
    private readonly IVehicleService _vehicleService;
    private readonly IPartyService _partyService;
    private readonly ILogger<UpdateVehicleCommandHandler> _logger;

    /// <summary>
    /// Creates the vehicle update command handler.
    /// </summary>
    /// <param name="vehicleService">The vehicle workflow service.</param>
    /// <param name="partyService">The service used to resolve the current landlord.</param>
    /// <param name="logger">The structured command logger.</param>
    public UpdateVehicleCommandHandler(IVehicleService vehicleService, IPartyService partyService, ILogger<UpdateVehicleCommandHandler> logger)
    {
        _vehicleService = vehicleService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Updates a vehicle under the current landlord scope.
    /// </summary>
    /// <param name="request">The validated partial vehicle update command.</param>
    /// <param name="cancellationToken">The token used to cancel the command.</param>
    /// <returns>The updated vehicle response envelope.</returns>
    public async ValueTask<ResponseDto<VehicleDetailResponseDto>> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        // Resolve landlord scope before applying the submitted partial update.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var vehicleRequest = request.Adapt<VehicleUpdateRequestModel>();
        vehicleRequest.CurrentParty = currentParty;

        // Apply the guarded mutation and return the refreshed detail projection.
        var response = await _vehicleService.UpdateAsync(vehicleRequest, cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.VehicleLogs.VEHICLE_UPDATE_COMMAND_COMPLETED, response.Id, currentParty.PartyPublicId);

        return new ResponseDto<VehicleDetailResponseDto>(response);
    }
}
