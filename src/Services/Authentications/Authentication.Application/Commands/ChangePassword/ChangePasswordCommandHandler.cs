namespace Authentication.Application.Commands.ChangePassword;

/// <summary>
/// Handles authenticated password-change requests.
/// </summary>
public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, ResponseDto<ChangePasswordResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuthService _authService;

    /// <summary>
    /// Creates the authenticated password-change handler with current-principal access and password update services.
    /// </summary>
    /// <param name="authenticationService">The service that verifies and changes the current password.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    public ChangePasswordCommandHandler(
        IAuthenticationService authenticationService,
        IAuthService authService)
    {
        _authenticationService = authenticationService;
        _authService = authService;
    }

    /// <summary>
    /// Handles logged-in password changes.
    /// </summary>
    /// <param name="request">The logged-in change-password payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the password-change success message.</returns>
    public async ValueTask<ResponseDto<ChangePasswordResponseDto>> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        // Authenticated password changes are account-level security updates and require the token public user id.
        var changeRequest = request.Adapt<ChangePasswordRequestDto>();
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);
        var currentSessionPublicId = _authService.SessionId()
                                     ?? throw new HttpStatusCodeException(
                                         ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                         UNAUTHORIZED,
                                         StatusCodes.Status401Unauthorized);

        // AuthenticationService owns password verification, hashing, refresh-token revocation, and transaction save.
        var result = await _authenticationService.ChangePasswordAsync(
            currentUserPublicId,
            currentSessionPublicId,
            changeRequest,
            cancellationToken);
        return new ResponseDto<ChangePasswordResponseDto>(result);
    }
}
