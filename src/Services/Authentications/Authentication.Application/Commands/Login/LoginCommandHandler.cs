namespace Authentication.Application.Commands.Login;

/// <summary>
/// Handles local username-and-password login requests.
/// </summary>
public class LoginCommandHandler : ICommandHandler<LoginCommand, ResponseDto<LoginResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILoginAttemptService _loginAttemptService;
    private readonly ILogger<LoginCommandHandler> _logger;

    /// <summary>
    /// Creates the local-login handler with credential verification services and flow logging.
    /// </summary>
    /// <param name="authenticationService">The service that verifies local credentials and issues tokens.</param>
    /// <param name="loginAttemptService">The service that tracks failed local-login attempts and temporary locks.</param>
    /// <param name="logger">The logger used for login flow completion tracking.</param>
    public LoginCommandHandler(
        IAuthenticationService authenticationService,
        ILoginAttemptService loginAttemptService,
        ILogger<LoginCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _loginAttemptService = loginAttemptService;
        _logger = logger;
    }

    /// <summary>
    /// Handles username/password login and returns the issued access/refresh token pair.
    /// </summary>
    /// <param name="request">The login command payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the login payload.</returns>
    public async ValueTask<ResponseDto<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Mapster owns request normalization after FluentValidation has accepted the payload shape.
        var loginRequest = request.Adapt<LoginRequestDto>();

        // Block locked usernames before password verification to avoid extending brute-force work.
        await _loginAttemptService.CheckAccountLockedAsync(loginRequest.UserName, cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.LoginLogs.LOCAL_LOGIN_FLOW_STEP1_LOCK_CHECKED);

        LoginResponseDto result;
        try
        {
            // AuthenticationService verifies credentials and persists the refresh-token session.
            result = await _authenticationService.LoginAsync(loginRequest, cancellationToken);
            _logger.LogInformation(ApplicationLogConstants.LoginLogs.LOCAL_LOGIN_FLOW_STEP2_CREDENTIALS_ACCEPTED);
        }
        catch (ApiException ex) when (ex.ErrorCode == ApplicationErrorConstants.AccountErrorCodes.AUTH_INVALID_CREDENTIALS)
        {
            // Count only credential failures after request validation has already accepted the payload.
            await _loginAttemptService.CountFailedAttemptAsync(loginRequest.UserName, cancellationToken);
            _logger.LogWarning(ex, ApplicationLogConstants.LoginLogs.LOCAL_LOGIN_FLOW_INVALID_ATTEMPT_COUNTED);

            // Recreate the public credential error after counting; do not use bare rethrow in handled flows.
            throw new ApiException(
                ApplicationErrorConstants.AccountErrors.INVALID_USERNAME_OR_PASSWORD_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_INVALID_CREDENTIALS,
                StatusCodes.Status401Unauthorized);
        }

        // Successful local login clears any previous failed-attempt state for the username.
        await _loginAttemptService.RemoveAttemptsAsync(loginRequest.UserName, cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.LoginLogs.LOCAL_LOGIN_FLOW_STEP3_ATTEMPTS_RESET);
        _logger.LogInformation(ApplicationLogConstants.LoginLogs.LOCAL_LOGIN_COMPLETED);

        return new ResponseDto<LoginResponseDto>(result);
    }
}
