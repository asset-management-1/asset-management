namespace Authentication.Application.Commands.CompleteExternalRegistration;

/// <summary>
/// Delegates validated external-registration completion to the provider-aware account service.
/// </summary>
public class CompleteExternalRegistrationCommandHandler : ICommandHandler<CompleteExternalRegistrationCommand, ResponseDto<LoginResponseDto>>
{
    private readonly IExternalAuthenticationService _externalAuthenticationService;
    private readonly ILogger<CompleteExternalRegistrationCommandHandler> _logger;

    /// <summary>
    /// Creates the complete external-registration handler.
    /// </summary>
    /// <param name="externalAuthenticationService">The service that revalidates provider identity and creates the account graph.</param>
    /// <param name="logger">The structured external-registration workflow logger.</param>
    public CompleteExternalRegistrationCommandHandler(
        IExternalAuthenticationService externalAuthenticationService,
        ILogger<CompleteExternalRegistrationCommandHandler> logger)
    {
        _externalAuthenticationService = externalAuthenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Maps validated client fields and completes registration after provider revalidation.
    /// </summary>
    /// <param name="request">The external-registration completion command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The standardized response containing the new login token pair.</returns>
    public async ValueTask<ResponseDto<LoginResponseDto>> Handle(
        CompleteExternalRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        // Map the validated flat client fields once before delegating provider revalidation and account creation.
        var registrationRequest = request.Adapt<CompleteExternalRegistrationRequestDto>();
        var result = await _externalAuthenticationService.CompleteRegistrationAsync(
            registrationRequest,
            cancellationToken);

        // Record the provider branch only; external credentials and returned tokens remain secret.
        _logger.LogInformation(
            ApplicationLogConstants.ExternalLogs.EXTERNAL_REGISTRATION_COMPLETED,
            request.Provider);

        return new ResponseDto<LoginResponseDto>(result);
    }
}
