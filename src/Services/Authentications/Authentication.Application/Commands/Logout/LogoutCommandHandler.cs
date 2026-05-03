namespace Authentication.Application.Commands.Logout;

public class LogoutCommandHandler : ICommandHandler<LogoutCommand, ResponseDto<string>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuthService _authService;

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
    public async Task<ResponseDto<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userIdRaw = _authService.UserId();
        if (!Guid.TryParse(userIdRaw, out var userPublicId))
        {
            return new ResponseDto<string>(AUTH_UNAUTHORIZED, UNAUTHORIZED_REQUEST_MESSAGE);
        }

        return await _authenticationService.LogoutAsync(
            userPublicId,
            request.RefreshToken,
            request.LogoutAllSessions,
            cancellationToken);
    }
}
