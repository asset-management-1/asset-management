namespace Authentication.Application.Commands.ChangeForgotPassword;

public class ChangeForgotPasswordCommandHandler : ICommandHandler<ChangeForgotPasswordCommand, ResponseDto<string>>
{
    private readonly IAuthenticationService _authenticationService;

    public ChangeForgotPasswordCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public Task<ResponseDto<string>> Handle(ChangeForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        return _authenticationService.ChangeForgotPasswordAsync(request, cancellationToken);
    }
}
