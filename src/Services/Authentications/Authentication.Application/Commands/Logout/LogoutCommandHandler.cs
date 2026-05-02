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
        if (!long.TryParse(userIdRaw, out var userId))
        {
            return new ResponseDto<string>(AUTH_UNAUTHORIZED, "Unauthorized request.");
        }

        return await _authenticationService.LogoutAsync(
            userId,
            request.RefreshToken,
            request.LogoutAllSessions,
            cancellationToken);
    }
}
