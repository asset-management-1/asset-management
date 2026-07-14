namespace Haven.Application.Commands.CreateVehicle;

/// <summary>
/// Coordinates landlord vehicle registration requests.
/// </summary>
public class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand, ResponseDto<VehicleDetailResponseDto>>
{
    private readonly IVehicleService _vehicleService;
    private readonly IPartyService _partyService;
    private readonly ILogger<CreateVehicleCommandHandler> _logger;

    /// <summary>
    /// Creates the vehicle registration command handler.
    /// </summary>
    /// <param name="vehicleService">The vehicle workflow service.</param>
    /// <param name="partyService">The service used to resolve the current landlord.</param>
    /// <param name="logger">The structured command logger.</param>
    public CreateVehicleCommandHandler(IVehicleService vehicleService, IPartyService partyService, ILogger<CreateVehicleCommandHandler> logger)
    {
        _vehicleService = vehicleService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a vehicle under the current landlord scope.
    /// </summary>
    /// <param name="request">The validated vehicle registration command.</param>
    /// <param name="cancellationToken">The token used to cancel the command.</param>
    /// <returns>The created vehicle response envelope.</returns>
    public async ValueTask<ResponseDto<VehicleDetailResponseDto>> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        // Resolve landlord scope before mapping multipart fields into the vehicle workflow request.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var vehicleRequest = request.Adapt<VehicleCreateRequestModel>();
        vehicleRequest.CurrentParty = currentParty;

        // The service validates the primary payer, uploads images, and persists the vehicle atomically.
        var response = await _vehicleService.CreateAsync(vehicleRequest, cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.VehicleLogs.VEHICLE_CREATE_COMMAND_COMPLETED, response.Id, currentParty.PartyPublicId);

        return new ResponseDto<VehicleDetailResponseDto>(response);
    }
}
