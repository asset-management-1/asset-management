namespace Authentication.Application.Commands.ThirdPartyLogin;

public class ThirdPartyLoginCommandHandler : ICommandHandler<ThirdPartyLoginCommand, ResponseDto<LoginResponse>>
{
    private readonly IAuthenticationService _authenticationService;

    public ThirdPartyLoginCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Handles third-party login and returns issued access/refresh tokens.
    /// </summary>
    public Task<ResponseDto<LoginResponse>> Handle(ThirdPartyLoginCommand request, CancellationToken cancellationToken)
    {
        return _authenticationService.LoginByThirdPartyAsync(request, cancellationToken);
    }
}
