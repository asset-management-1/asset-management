namespace Authentication.Application.Commands.Register;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, ResponseDto<string>>
{
    private readonly IAuthenticationService _authenticationService;

    public RegisterCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Handles user registration and returns OTP send result.
    /// </summary>
    public Task<ResponseDto<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return _authenticationService.RegisterAsync(request, cancellationToken);
    }
}
