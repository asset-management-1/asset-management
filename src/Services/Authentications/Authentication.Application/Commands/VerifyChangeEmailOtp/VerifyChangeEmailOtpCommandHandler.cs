namespace Authentication.Application.Commands.VerifyChangeEmailOtp;

/// <summary>
/// Handles verification of change-email session OTP values.
/// </summary>
public class VerifyChangeEmailOtpCommandHandler : ICommandHandler<VerifyChangeEmailOtpCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IUserService _userService;
    private readonly ICachingService _cachingService;
    private readonly IAuthService _authService;
    private readonly ILogger<VerifyChangeEmailOtpCommandHandler> _logger;

    /// <summary>
    /// Creates the change-email verification handler with change-email session cache and account update services.
    /// </summary>
    /// <param name="userService">The service that applies verified email changes.</param>
    /// <param name="cachingService">The cache service used for change-email session state.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    /// <param name="logger">The logger used for email-change verification tracking.</param>
    public VerifyChangeEmailOtpCommandHandler(
        IUserService userService,
        ICachingService cachingService,
        IAuthService authService,
        ILogger<VerifyChangeEmailOtpCommandHandler> logger)
    {
        _userService = userService;
        _cachingService = cachingService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Verifies the current change-email OTP and updates the email when valid.
    /// </summary>
    /// <param name="request">The change-email OTP verification payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the email-change result.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        VerifyChangeEmailOtpCommand request,
        CancellationToken cancellationToken)
    {
        // Session state must match the submitted email so an older OTP cannot verify a newer target.
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);
        var verifyRequest = request.Adapt<VerifyChangeEmailOtpRequestDto>();
        var normalizedEmail = verifyRequest.NewEmail;
        var changeEmailSessionKey = AuthenticationFlowHelper.BuildChangeEmailSessionKey(currentUserPublicId);
        var changeEmailSession = await _cachingService.GetAsync<ChangeEmailSessionCacheResponseDto>(
            changeEmailSessionKey,
            cancellationToken);
        if (changeEmailSession is null
            || changeEmailSession.Otp is null
            || !string.Equals(changeEmailSession.NewEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(CHANGE_EMAIL_SESSION_EXPIRED_MESSAGE, AUTH_RESET_SESSION_INVALID);
        }

        _logger.LogInformation(VERIFY_CHANGE_EMAIL_FLOW_STEP1_SESSION_VALIDATED);

        await VerifyOtpAsync(changeEmailSessionKey, changeEmailSession, verifyRequest.Otp, cancellationToken);
        _logger.LogInformation(VERIFY_CHANGE_EMAIL_FLOW_STEP2_OTP_VERIFIED);

        // Only after OTP ownership is confirmed do we update the account email and linked party snapshots.
        var result = await _userService.VerifyChangeEmailAsync(
            verifyRequest,
            currentUserPublicId,
            cancellationToken);
        _logger.LogInformation(VERIFY_CHANGE_EMAIL_FLOW_STEP3_EMAIL_UPDATED);

        // Cleanup removes both session state and cooldown so a later email-change flow starts fresh.
        await _cachingService.RemoveAsync(changeEmailSessionKey, cancellationToken);
        await _cachingService.RemoveAsync(
            AuthenticationFlowHelper.BuildChangeEmailCooldownKey(currentUserPublicId),
            cancellationToken);
        _logger.LogInformation(VERIFY_CHANGE_EMAIL_FLOW_STEP4_STATE_CLEANED);
        _logger.LogInformation(VERIFY_CHANGE_EMAIL_FLOW_COMPLETED, currentUserPublicId);

        return new ResponseDto<OperationStatusResponseDto>(result);
    }

    /// <summary>
    /// Verifies the current change-email OTP against the cached session.
    /// </summary>
    /// <param name="changeEmailSessionKey">The cache key that stores the change-email session.</param>
    /// <param name="changeEmailSession">The cached change-email session containing the target email and OTP state.</param>
    /// <param name="otp">The OTP supplied by the caller.</param>
    /// <param name="cancellationToken">The token used to cancel cache operations.</param>
    /// <returns>A task that completes when the OTP is valid.</returns>
    private async Task VerifyOtpAsync(
        string changeEmailSessionKey,
        ChangeEmailSessionCacheResponseDto changeEmailSession,
        string otp,
        CancellationToken cancellationToken)
    {
        var otpEntry = changeEmailSession.Otp;
        if (otpEntry is null || string.IsNullOrWhiteSpace(otpEntry.Code))
        {
            throw new ApiException(OTP_INVALID_OR_EXPIRED_MESSAGE, AUTH_OTP_INVALID);
        }

        var remainingTtl = otpEntry.ExpiresAtUtc - DateTime.UtcNow;
        if (remainingTtl <= TimeSpan.Zero)
        {
            await _cachingService.RemoveAsync(changeEmailSessionKey, cancellationToken);
            throw new ApiException(OTP_INVALID_OR_EXPIRED_MESSAGE, AUTH_OTP_INVALID);
        }

        if (!string.Equals(otpEntry.Code, otp, StringComparison.Ordinal))
        {
            // Persist failed attempts so repeated incorrect OTP values eventually consume the current OTP.
            otpEntry.Attempts++;
            if (otpEntry.Attempts >= OTP_MAX_VERIFY_ATTEMPTS)
            {
                await _cachingService.RemoveAsync(changeEmailSessionKey, cancellationToken);
                _logger.LogInformation(VERIFY_CHANGE_EMAIL_FLOW_OTP_LOCKED);
            }
            else
            {
                await _cachingService.SetAbsoluteAsync(
                    changeEmailSessionKey,
                    changeEmailSession,
                    remainingTtl,
                    cancellationToken);
                _logger.LogInformation(VERIFY_CHANGE_EMAIL_FLOW_OTP_ATTEMPT_RECORDED);
            }

            throw new ApiException(OTP_INVALID_OR_EXPIRED_MESSAGE, AUTH_OTP_INVALID);
        }
    }
}
