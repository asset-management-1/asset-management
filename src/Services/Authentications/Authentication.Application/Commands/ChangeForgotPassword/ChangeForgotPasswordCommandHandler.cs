namespace Authentication.Application.Commands.ChangeForgotPassword;

/// <summary>
/// Handles password changes after forgot-password OTP verification.
/// </summary>
public class ChangeForgotPasswordCommandHandler : ICommandHandler<ChangeForgotPasswordCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICachingService _cachingService;
    private readonly ILogger<ChangeForgotPasswordCommandHandler> _logger;

    /// <summary>
    /// Creates the forgot-password password-change handler with reset-session cache and password update services.
    /// </summary>
    /// <param name="authenticationService">The service that changes the password after reset-session validation.</param>
    /// <param name="cachingService">The cache service used for reset-session state.</param>
    /// <param name="logger">The logger used for forgot-password change completion tracking.</param>
    public ChangeForgotPasswordCommandHandler(
        IAuthenticationService authenticationService,
        ICachingService cachingService,
        ILogger<ChangeForgotPasswordCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _cachingService = cachingService;
        _logger = logger;
    }

    /// <summary>
    /// Handles password changes after forgot-password OTP verification.
    /// </summary>
    /// <param name="request">The forgot-password change-password payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the password-change success message.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        ChangeForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // The reset-session cache proves the user already passed forgot-password OTP verification.
        var changeRequest = request.Adapt<ChangeForgotPasswordRequestDto>();
        var normalizedEmail = changeRequest.Email;
        var resetSessionKey = string.Format(RESET_SESSION_KEY_PATTERN, normalizedEmail);
        var resetSession = await _cachingService.GetAsync<ResetSessionCacheResponseDto>(
            resetSessionKey,
            cancellationToken);
        if (resetSession is null)
        {
            throw new ApiException(ApplicationErrorConstants.OtpErrors.RESET_SESSION_INVALID_MESSAGE, ApplicationErrorConstants.OtpErrorCodes.AUTH_RESET_SESSION_INVALID);
        }

        _logger.LogInformation(ApplicationLogConstants.ForgotPasswordLogs.CHANGE_FORGOT_PASSWORD_FLOW_STEP1_RESET_SESSION_VALIDATED);

        // Password update and refresh-token revocation are delegated to the auth service transaction boundary.
        var result = await _authenticationService.ChangeForgotPasswordAsync(
            changeRequest,
            cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.ForgotPasswordLogs.CHANGE_FORGOT_PASSWORD_FLOW_STEP2_PASSWORD_CHANGED);

        // Consume the reset session only after the password has been changed successfully.
        await _cachingService.RemoveAsync(resetSessionKey, cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.ForgotPasswordLogs.CHANGE_FORGOT_PASSWORD_FLOW_STEP3_RESET_SESSION_CONSUMED);
        _logger.LogInformation(ApplicationLogConstants.ForgotPasswordLogs.FORGOT_PASSWORD_CHANGE_COMPLETED);

        return new ResponseDto<OperationStatusResponseDto>(result);
    }
}
