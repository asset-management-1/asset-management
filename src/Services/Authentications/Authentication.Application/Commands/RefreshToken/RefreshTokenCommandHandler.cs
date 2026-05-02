namespace Authentication.Application.Commands.RefreshToken;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, ResponseDto<LoginResponse>>
{
    private readonly IAuthenticationService _authenticationService;

    public RefreshTokenCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Handles refresh-token exchange and issues a new access/refresh token pair.
    /// </summary>
    public Task<ResponseDto<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return _authenticationService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
    }
}
