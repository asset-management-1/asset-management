namespace Authentication.Application.Commands.ChangeForgotPassword;

/// <summary>
/// Handles password changes after forgot-password OTP verification.
/// </summary>
public class ChangeForgotPasswordCommandHandler : ICommandHandler<ChangeForgotPasswordCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICachingService _cachingService;
    private readonly IAtomicCacheService _atomicCacheService;
    private readonly ILogger<ChangeForgotPasswordCommandHandler> _logger;

    /// <summary>
    /// Creates the forgot-password password-change handler with reset-session cache and password update services.
    /// </summary>
    /// <param name="authenticationService">The service that changes the password after reset-session validation.</param>
    /// <param name="cachingService">The cache service used for reset-session state.</param>
    /// <param name="atomicCacheService">The atomic cache service used to consume a reset token once.</param>
    /// <param name="logger">The structured forgot-password mutation logger.</param>
    public ChangeForgotPasswordCommandHandler(
        IAuthenticationService authenticationService,
        ICachingService cachingService,
        IAtomicCacheService atomicCacheService,
        ILogger<ChangeForgotPasswordCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _cachingService = cachingService;
        _atomicCacheService = atomicCacheService;
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
        // Step 1: Load unexpired reset authority created by successful forgot-password OTP verification.
        var resetSessionKey = string.Format(RESET_SESSION_KEY_PATTERN, request.PasswordResetToken);
        var resetSession = await _cachingService.GetAsync<ResetSessionCacheResponseDto>(
            resetSessionKey,
            cancellationToken);
        if (resetSession is null || resetSession.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new ApiException(ApplicationErrorConstants.OtpErrors.RESET_SESSION_INVALID_MESSAGE, ApplicationErrorConstants.OtpErrorCodes.AUTH_RESET_SESSION_INVALID);
        }

        // Step 2: Atomically consume the opaque reset token so only one password mutation can proceed.
        var remainingTtl = resetSession.ExpiresAtUtc - DateTime.UtcNow;
        var consumed = await _atomicCacheService.TrySetIfAbsentAsync(
            string.Format(OTP_CONSUME_KEY_PATTERN, resetSessionKey),
            "1",
            remainingTtl,
            cancellationToken);
        if (!consumed)
        {
            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.RESET_SESSION_INVALID_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_RESET_SESSION_INVALID);
        }

        // Step 3: Remove reset authority before DB mutation; a later failure requires a fresh forgot-password flow.
        await _cachingService.RemoveAsync(resetSessionKey, cancellationToken);

        // Step 4: Change the password and revoke sessions inside the authentication transaction boundary.
        var result = await _authenticationService.ChangeForgotPasswordAsync(
            new ChangeForgotPasswordRequestDto
            {
                Email = resetSession.Email,
                NewPassword = request.NewPassword
            },
            cancellationToken);

        // Do not log email, reset token, or password; record only the completed security transition.
        _logger.LogInformation(ApplicationLogConstants.PasswordLogs.FORGOT_PASSWORD_CHANGED);

        return new ResponseDto<OperationStatusResponseDto>(result);
    }
}
