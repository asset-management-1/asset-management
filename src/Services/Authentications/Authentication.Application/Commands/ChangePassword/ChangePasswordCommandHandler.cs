namespace Authentication.Application.Commands.ChangePassword;

public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, ResponseDto<string>>
{
    private readonly IAuthenticationService _authenticationService;

    public ChangePasswordCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public Task<ResponseDto<string>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        return _authenticationService.ChangePasswordAsync(request, cancellationToken);
    }
}
