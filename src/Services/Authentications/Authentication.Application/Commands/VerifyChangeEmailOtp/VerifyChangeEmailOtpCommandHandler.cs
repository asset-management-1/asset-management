namespace Authentication.Application.Commands.VerifyChangeEmailOtp;

/// <summary>
/// Handles verification of change-email session OTP values.
/// </summary>
public class VerifyChangeEmailOtpCommandHandler : ICommandHandler<VerifyChangeEmailOtpCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IUserService _userService;
    private readonly ICachingService _cachingService;
    private readonly IAtomicCacheService _atomicCacheService;
    private readonly IAuthService _authService;
    private readonly ILogger<VerifyChangeEmailOtpCommandHandler> _logger;

    /// <summary>
    /// Creates the change-email verification handler with change-email session cache and account update services.
    /// </summary>
    /// <param name="userService">The service that applies verified email changes.</param>
    /// <param name="cachingService">The cache service used for change-email session state.</param>
    /// <param name="atomicCacheService">The atomic cache service used for attempts and one-time OTP consumption.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    /// <param name="logger">The logger used for email-change verification tracking.</param>
    public VerifyChangeEmailOtpCommandHandler(
        IUserService userService,
        ICachingService cachingService,
        IAtomicCacheService atomicCacheService,
        IAuthService authService,
        ILogger<VerifyChangeEmailOtpCommandHandler> logger)
    {
        _userService = userService;
        _cachingService = cachingService;
        _atomicCacheService = atomicCacheService;
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
        // Step 1: Match the authenticated user, target email, and cached session before OTP verification.
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);
        var verifyRequest = request.Adapt<VerifyChangeEmailOtpRequestDto>();
        var normalizedEmail = verifyRequest.NewEmail;
        var changeEmailSessionKey = string.Format(CHANGE_EMAIL_SESSION_KEY_PATTERN, currentUserPublicId);
        var changeEmailSession = await _cachingService.GetAsync<ChangeEmailSessionCacheResponseDto>(
            changeEmailSessionKey,
            cancellationToken);
        if (changeEmailSession is null
            || changeEmailSession.Otp is null
            || !string.Equals(changeEmailSession.NewEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(ApplicationErrorConstants.ProfileErrors.CHANGE_EMAIL_SESSION_EXPIRED_MESSAGE, ApplicationErrorConstants.OtpErrorCodes.AUTH_RESET_SESSION_INVALID);
        }

        _logger.LogInformation(ApplicationLogConstants.ChangeEmailLogs.VERIFY_CHANGE_EMAIL_FLOW_STEP1_SESSION_VALIDATED);

        // Step 2: Verify and atomically claim the OTP before changing account data.
        var consumeKey = await VerifyOtpAsync(
            changeEmailSessionKey,
            changeEmailSession,
            verifyRequest.Otp,
            cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.ChangeEmailLogs.VERIFY_CHANGE_EMAIL_FLOW_STEP2_OTP_VERIFIED);

        OperationStatusResponseDto result;
        var emailUpdated = false;

        try
        {
            // Step 3: Update the account email only after this request owns the OTP claim.
            result = await _userService.VerifyChangeEmailAsync(
                verifyRequest,
                currentUserPublicId,
                cancellationToken);

            emailUpdated = true;
        }
        finally
        {
            if (!emailUpdated)
            {
                // Step 3a: Release the OTP claim after an unsuccessful email update so the request may be retried.
                try
                {
                    await _atomicCacheService.RemoveAsync(consumeKey, CancellationToken.None);
                }
                catch (Exception cleanupException)
                {
                    _logger.LogWarning(
                        cleanupException,
                        ApplicationLogConstants.ChangeEmailLogs.VERIFY_CHANGE_EMAIL_FLOW_CONSUME_CLEANUP_FAILED);
                }
            }
        }

        _logger.LogInformation(ApplicationLogConstants.ChangeEmailLogs.VERIFY_CHANGE_EMAIL_FLOW_STEP3_EMAIL_UPDATED);

        // Step 4: Remove session, cooldown, and attempt state after persistence succeeds.
        await _cachingService.RemoveAsync(changeEmailSessionKey, cancellationToken);
        await _atomicCacheService.RemoveAsync(
            string.Format(CHANGE_EMAIL_COOLDOWN_KEY_PATTERN, currentUserPublicId),
            cancellationToken);
        await _atomicCacheService.RemoveAsync(
            string.Format(OTP_VERIFY_ATTEMPT_KEY_PATTERN, changeEmailSessionKey),
            cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.ChangeEmailLogs.VERIFY_CHANGE_EMAIL_FLOW_STEP4_STATE_CLEANED);
        _logger.LogInformation(ApplicationLogConstants.ChangeEmailLogs.VERIFY_CHANGE_EMAIL_FLOW_COMPLETED, currentUserPublicId);

        return new ResponseDto<OperationStatusResponseDto>(result);
    }

    /// <summary>
    /// Verifies the current change-email OTP against the cached session.
    /// </summary>
    /// <param name="changeEmailSessionKey">The cache key that stores the change-email session.</param>
    /// <param name="changeEmailSession">The cached change-email session containing the target email and OTP state.</param>
    /// <param name="otp">The OTP supplied by the caller.</param>
    /// <param name="cancellationToken">The token used to cancel cache operations.</param>
    /// <returns>The atomic consume-marker key owned by the successful request.</returns>
    private async Task<string> VerifyOtpAsync(
        string changeEmailSessionKey,
        ChangeEmailSessionCacheResponseDto changeEmailSession,
        string otp,
        CancellationToken cancellationToken)
    {
        // Step 1: Read the cached OTP once for every validation branch below.
        var otpEntry = changeEmailSession.Otp;

        // Step 2: Reject incomplete state before computing expiry or attempts.
        if (otpEntry is null || string.IsNullOrWhiteSpace(otpEntry.Code))
        {
            throw new ApiException(ApplicationErrorConstants.OtpErrors.OTP_INVALID_OR_EXPIRED_MESSAGE, ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_INVALID);
        }

        var remainingTtl = otpEntry.ExpiresAtUtc - DateTime.UtcNow;

        // Step 3: Remove expired state before rejecting it so stale sessions cannot be retried.
        if (remainingTtl <= TimeSpan.Zero)
        {
            await _cachingService.RemoveAsync(changeEmailSessionKey, cancellationToken);
            throw new ApiException(ApplicationErrorConstants.OtpErrors.OTP_INVALID_OR_EXPIRED_MESSAGE, ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_INVALID);
        }

        if (otpEntry.Code != otp)
        {
            // Step 4: Count failures atomically so concurrent submissions cannot bypass the limit.
            var attemptKey = string.Format(OTP_VERIFY_ATTEMPT_KEY_PATTERN, changeEmailSessionKey);
            var attempts = await _atomicCacheService.IncrementAsync(
                attemptKey,
                remainingTtl,
                cancellationToken);
            if (attempts >= OTP_MAX_VERIFY_ATTEMPTS)
            {
                await _cachingService.RemoveAsync(changeEmailSessionKey, cancellationToken);
                await _atomicCacheService.RemoveAsync(attemptKey, cancellationToken);
                _logger.LogInformation(ApplicationLogConstants.ChangeEmailLogs.VERIFY_CHANGE_EMAIL_FLOW_OTP_LOCKED);
            }
            else
            {
                _logger.LogInformation(ApplicationLogConstants.ChangeEmailLogs.VERIFY_CHANGE_EMAIL_FLOW_OTP_ATTEMPT_RECORDED);
            }

            throw new ApiException(ApplicationErrorConstants.OtpErrors.OTP_INVALID_OR_EXPIRED_MESSAGE, ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_INVALID);
        }

        // Step 5: Claim the valid OTP atomically so only one request may update the email.
        var consumeKey = string.Format(OTP_CONSUME_KEY_PATTERN, changeEmailSessionKey);
        var acquired = await _atomicCacheService.TrySetIfAbsentAsync(
            consumeKey,
            "1",
            remainingTtl,
            cancellationToken);

        if (!acquired)
        {
            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_INVALID_OR_EXPIRED_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_INVALID);
        }

        return consumeKey;
    }
}
