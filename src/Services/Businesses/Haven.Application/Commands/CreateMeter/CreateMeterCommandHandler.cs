namespace Haven.Application.Commands.CreateMeter;

/// <summary>
/// Coordinates creation of one confirmed room meter period.
/// </summary>
public class CreateMeterCommandHandler
    : ICommandHandler<CreateMeterCommand, ResponseDto<MeterPeriodDetailResponseDto>>
{
    private readonly IRoomMeterService _meterService;
    private readonly IPartyService _partyService;
    private readonly ILogger<CreateMeterCommandHandler> _logger;

    /// <summary>
    /// Creates the room meter command handler.
    /// </summary>
    /// <param name="meterService">The room meter workflow service.</param>
    /// <param name="partyService">The service used to resolve the active landlord.</param>
    /// <param name="logger">The structured command logger.</param>
    public CreateMeterCommandHandler(
        IRoomMeterService meterService,
        IPartyService partyService,
        ILogger<CreateMeterCommandHandler> logger)
    {
        _meterService = meterService;
        _partyService = partyService;
        _logger = logger;
    }

    /// <summary>
    /// Creates and confirms the submitted room meter period.
    /// </summary>
    /// <param name="request">The validated meter command.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The committed meter period response.</returns>
    public async ValueTask<ResponseDto<MeterPeriodDetailResponseDto>> Handle(
        CreateMeterCommand request,
        CancellationToken cancellationToken)
    {
        // Resolve landlord scope before mapping the submitted values into the service request.
        var currentParty = await _partyService.GetCurrentLandlordAsync(cancellationToken);
        var serviceRequest = request.Adapt<MeterMutationRequestModel>();
        serviceRequest.CurrentParty = currentParty;

        // The service rechecks room access, policies, prior readings, invoices, and upload safety atomically.
        var response = await _meterService.SaveConfirmedPeriodAsync(
            serviceRequest,
            MeterMutationModeEnum.Create,
            cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.MeterLogs.MUTATION_COMPLETED,
            request.RoomId,
            request.BillingDate);

        return new ResponseDto<MeterPeriodDetailResponseDto>(response);
    }
}
