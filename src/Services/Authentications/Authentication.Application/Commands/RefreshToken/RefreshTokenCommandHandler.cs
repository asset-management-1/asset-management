namespace Authentication.Application.Commands.RefreshToken;

/// <summary>
/// Handles refresh-token exchange requests.
/// </summary>
public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, ResponseDto<LoginResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    /// <summary>
    /// Creates the refresh-token handler with token rotation services.
    /// </summary>
    /// <param name="authenticationService">The service that validates and rotates refresh tokens.</param>
    /// <param name="logger">The structured refresh-token workflow logger.</param>
    public RefreshTokenCommandHandler(
        IAuthenticationService authenticationService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Handles refresh-token exchange and issues a new access/refresh token pair.
    /// </summary>
    /// <param name="request">The refresh-token command payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the refreshed login payload.</returns>
    public async ValueTask<ResponseDto<LoginResponseDto>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Refresh-token rotation is delegated to infrastructure because it validates token hashes and persists the new session.
        var result = await _authenticationService.RefreshTokenAsync(
            request.Adapt<RefreshTokenRequestDto>(),
            cancellationToken);

        // Never log the submitted or issued token values; only record successful rotation.
        _logger.LogInformation(ApplicationLogConstants.SessionLogs.REFRESH_TOKEN_ROTATED);

        return new ResponseDto<LoginResponseDto>(result);
    }
}
