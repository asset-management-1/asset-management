namespace Authentication.Application.Commands.ChangePassword;

/// <summary>
/// Handles authenticated password-change requests.
/// </summary>
public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuthService _authService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    /// <summary>
    /// Creates the authenticated password-change handler with current-principal access and password update services.
    /// </summary>
    /// <param name="authenticationService">The service that verifies and changes the current password.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    /// <param name="logger">The logger used for password-change flow tracking.</param>
    public ChangePasswordCommandHandler(
        IAuthenticationService authenticationService,
        IAuthService authService,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Handles logged-in password changes.
    /// </summary>
    /// <param name="request">The logged-in change-password payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the password-change success message.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // Authenticated password changes are account-level security updates and require the token public user id.
        var changeRequest = request.Adapt<ChangePasswordRequestDto>();
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);

        // AuthenticationService owns password verification, hashing, refresh-token revocation, and transaction save.
        var result = await _authenticationService.ChangePasswordAsync(
            currentUserPublicId,
            changeRequest,
            cancellationToken);
        _logger.LogInformation(PASSWORD_CHANGED, currentUserPublicId);

        return new ResponseDto<OperationStatusResponseDto>(result);
    }
}
