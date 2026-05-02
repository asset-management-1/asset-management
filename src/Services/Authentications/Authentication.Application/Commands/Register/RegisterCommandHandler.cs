namespace Authentication.Application.Commands.Register;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, ResponseDto<LoginResponse>>
{
    private readonly IAuthenticationService _authenticationService;

    public RegisterCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Handles user registration and returns issued access/refresh tokens.
    /// </summary>
    public Task<ResponseDto<LoginResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return _authenticationService.RegisterAsync(request, cancellationToken);
    }
}
