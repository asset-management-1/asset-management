namespace Authentication.Application.Commands.Logout;

/// <summary>
/// Handles the logout command.
/// </summary>
public class LogoutCommandHandler : ICommandHandler<LogoutCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuthService _authService;

    /// <summary>
    /// Creates the logout handler with session revocation services.
    /// </summary>
    /// <param name="authenticationService">The service that revokes refresh-token state.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    public LogoutCommandHandler(
        IAuthenticationService authenticationService,
        IAuthService authService)
    {
        _authenticationService = authenticationService;
        _authService = authService;
    }

    /// <summary>
    /// Handles logout by revoking the refresh token(s) of the current authenticated user.
    /// </summary>
    /// <param name="request">The logout command payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the logout result.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        // Logout revocation always targets the authenticated account and session from the token.
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);
        var currentSessionPublicId = _authService.SessionId()
                                     ?? throw new HttpStatusCodeException(
                                         ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                         UNAUTHORIZED,
                                         StatusCodes.Status401Unauthorized);

        // The service revokes only the current client session; credential changes own global revocation.
        var result = await _authenticationService.LogoutAsync(
            currentUserPublicId,
            currentSessionPublicId,
            cancellationToken);
        return new ResponseDto<OperationStatusResponseDto>(result);
    }
}
