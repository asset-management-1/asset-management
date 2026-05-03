namespace Authentication.Application.Commands.VerifyRegisterEmail;

public class VerifyRegisterEmailCommandHandler : ICommandHandler<VerifyRegisterEmailCommand, ResponseDto<string>>
{
    private readonly IAuthenticationService _authenticationService;

    public VerifyRegisterEmailCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public Task<ResponseDto<string>> Handle(VerifyRegisterEmailCommand request, CancellationToken cancellationToken)
    {
        return _authenticationService.VerifyRegisterEmailAsync(request, cancellationToken);
    }
}
