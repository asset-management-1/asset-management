namespace Authentication.Application.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, ResponseDto<string>>
{
    private readonly IAuthenticationService _authenticationService;

    public ForgotPasswordCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Handles forgot-password flow by resetting password for the matched user account.
    /// </summary>
    public Task<ResponseDto<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        return _authenticationService.ForgotPasswordAsync(request, cancellationToken);
    }
}
