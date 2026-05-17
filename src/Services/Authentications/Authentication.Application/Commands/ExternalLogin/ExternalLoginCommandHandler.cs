namespace Authentication.Application.Commands.ExternalLogin;

/// <summary>
/// Handles external identity-provider login requests.
/// </summary>
public class ExternalLoginCommandHandler : ICommandHandler<ExternalLoginCommand, ResponseDto<LoginResponseDto>>
{
    private readonly IExternalAuthenticationService _externalAuthenticationService;
    private readonly ILogger<ExternalLoginCommandHandler> _logger;

    /// <summary>
    /// Creates the external-login handler with provider validation services and flow logging.
    /// </summary>
    /// <param name="externalAuthenticationService">The service that validates external identity tokens and issues Haven tokens.</param>
    /// <param name="logger">The logger used for external-login flow completion tracking.</param>
    public ExternalLoginCommandHandler(
        IExternalAuthenticationService externalAuthenticationService,
        ILogger<ExternalLoginCommandHandler> logger)
    {
        _externalAuthenticationService = externalAuthenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Handles external login and returns issued access/refresh tokens.
    /// </summary>
    /// <param name="request">The external-login payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the external-login payload.</returns>
    public async ValueTask<ResponseDto<LoginResponseDto>> Handle(
        ExternalLoginCommand request,
        CancellationToken cancellationToken)
    {
        // The infrastructure service validates the provider token, provisions missing accounts, and issues Haven tokens.
        var result = await _externalAuthenticationService.LoginAsync(
            request.Adapt<ExternalLoginRequestDto>(),
            cancellationToken);
        _logger.LogInformation(EXTERNAL_LOGIN_COMPLETED, request.Provider);

        return new ResponseDto<LoginResponseDto>(result);
    }
}
