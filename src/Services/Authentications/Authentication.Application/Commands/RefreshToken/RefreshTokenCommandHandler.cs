namespace Authentication.Application.Commands.RefreshToken;

/// <summary>
/// Handles refresh-token exchange requests.
/// </summary>
public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, ResponseDto<LoginResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;

    /// <summary>
    /// Creates the refresh-token handler with token rotation services.
    /// </summary>
    /// <param name="authenticationService">The service that validates and rotates refresh tokens.</param>
    public RefreshTokenCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
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
        return new ResponseDto<LoginResponseDto>(result);
    }
}
