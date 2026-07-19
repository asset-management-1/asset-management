namespace Authentication.Application.Commands.ExternalLogin;

/// <summary>
/// Handles external identity-provider login requests.
/// </summary>
public class ExternalLoginCommandHandler : ICommandHandler<ExternalLoginCommand, ResponseDto<ExternalLoginResponseDto>>
{
    private readonly IExternalAuthenticationService _externalAuthenticationService;
    private readonly ILogger<ExternalLoginCommandHandler> _logger;

    /// <summary>
    /// Creates the external-login handler with provider validation services.
    /// </summary>
    /// <param name="externalAuthenticationService">The service that validates provider identity and resolves the external-login business branch.</param>
    /// <param name="logger">The structured external-login workflow logger.</param>
    public ExternalLoginCommandHandler(
        IExternalAuthenticationService externalAuthenticationService,
        ILogger<ExternalLoginCommandHandler> logger)
    {
        _externalAuthenticationService = externalAuthenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Handles external login through an existing mapping, authoritative-email auto-link, or first-time registration prefill.
    /// </summary>
    /// <param name="request">The external-login payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the external-login payload.</returns>
    public async ValueTask<ResponseDto<ExternalLoginResponseDto>> Handle(
        ExternalLoginCommand request,
        CancellationToken cancellationToken)
    {
        // Existing mappings log in, authoritative email may auto-link, and first-time identities return prefill without DB/cache writes.
        var result = await _externalAuthenticationService.LoginAsync(
            request.Adapt<ExternalLoginRequestDto>(),
            cancellationToken);

        // Log the selected business branch without recording the external token or provider profile.
        _logger.LogInformation(
            ApplicationLogConstants.ExternalLogs.EXTERNAL_LOGIN_COMPLETED,
            request.Provider,
            result.IsNewRegistration);

        return new ResponseDto<ExternalLoginResponseDto>(result);
    }
}
