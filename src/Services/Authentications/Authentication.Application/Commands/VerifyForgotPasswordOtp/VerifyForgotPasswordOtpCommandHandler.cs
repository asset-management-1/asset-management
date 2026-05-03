namespace Authentication.Application.Commands.VerifyForgotPasswordOtp;

public class VerifyForgotPasswordOtpCommandHandler : ICommandHandler<VerifyForgotPasswordOtpCommand, ResponseDto<string>>
{
    private readonly IAuthenticationService _authenticationService;

    public VerifyForgotPasswordOtpCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public Task<ResponseDto<string>> Handle(VerifyForgotPasswordOtpCommand request, CancellationToken cancellationToken)
    {
        return _authenticationService.VerifyForgotPasswordOtpAsync(request, cancellationToken);
    }
}
