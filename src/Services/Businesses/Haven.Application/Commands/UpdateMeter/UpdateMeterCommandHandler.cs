namespace Haven.Application.Commands.UpdateMeter;

/// <summary>
/// Coordinates updates to one confirmed room meter period.
/// </summary>
public class UpdateMeterCommandHandler
    : ICommandHandler<UpdateMeterCommand, ResponseDto<MeterPeriodDetailResponseDto>>
{
    private readonly IRoomMeterService _meterService;
    private readonly IPartyService _partyService;
    private readonly ILogger<UpdateMeterCommandHandler> _logger;

    /// <summary>
    /// Creates the room meter update handler.
    /// </summary>
    /// <param name="meterService">The room meter workflow service.</param>
    /// <param name="partyService">The service used to resolve the active landlord.</param>
    /// <param name="logger">The structured command logger.</param>
    public UpdateMeterCommandHandler(
        IRoomMeterService meterService,
        IPartyService partyService,
        ILogger<UpdateMeterCommandHandler> logger)
    {
        _meterService = meterService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Updates and reconfirms the selected room meter period.
    /// </summary>
    /// <param name="request">The validated meter command.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The committed meter period response.</returns>
    public async ValueTask<ResponseDto<MeterPeriodDetailResponseDto>> Handle(
        UpdateMeterCommand request,
        CancellationToken cancellationToken)
    {
        // Resolve landlord scope before mapping partial values into the service request.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var serviceRequest = request.Adapt<MeterMutationRequestModel>();
        serviceRequest.CurrentParty = currentParty;

        // The service rechecks live invoice and payment state instead of trusting an earlier GET response.
        var response = await _meterService.SaveConfirmedPeriodAsync(
            serviceRequest,
            MeterMutationModeEnum.Update,
            cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.MeterLogs.MUTATION_COMPLETED,
            request.RoomId,
            request.BillingDate);

        return new ResponseDto<MeterPeriodDetailResponseDto>(response);
    }
}
