namespace Authentication.Application.Commands.VerifyForgotPasswordOtp;

/// <summary>
/// Handles forgot-password OTP verification requests.
/// </summary>
public class VerifyForgotPasswordOtpCommandHandler : ICommandHandler<VerifyForgotPasswordOtpCommand, ResponseDto<PasswordResetTokenResponseDto>>
{
    private readonly ICachingService _cachingService;
    private readonly IAtomicCacheService _atomicCacheService;
    private readonly ILogger<VerifyForgotPasswordOtpCommandHandler> _logger;

    /// <summary>
    /// Creates the forgot-password OTP verification handler with OTP cache and reset-session cache services.
    /// </summary>
    /// <param name="cachingService">The cache service used for OTP and reset-session state.</param>
    /// <param name="atomicCacheService">The atomic cache service used for attempts and one-time OTP consumption.</param>
    /// <param name="logger">The logger used for OTP verification flow tracking.</param>
    public VerifyForgotPasswordOtpCommandHandler(
        ICachingService cachingService,
        IAtomicCacheService atomicCacheService,
        ILogger<VerifyForgotPasswordOtpCommandHandler> logger)
    {
        _cachingService = cachingService;
        _atomicCacheService = atomicCacheService;
        _logger = logger;
    }

    /// <summary>
    /// Handles forgot-password OTP verification and opens a reset session.
    /// </summary>
    /// <param name="request">The forgot-password OTP verification payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the OTP-verification success message.</returns>
    public async ValueTask<ResponseDto<PasswordResetTokenResponseDto>> Handle(
        VerifyForgotPasswordOtpCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Normalise the request before resolving OTP and reset-session keys.
        var verifyRequest = request.Adapt<VerifyForgotPasswordOtpRequestDto>();
        var normalizedEmail = verifyRequest.Email;

        // Step 2: Verify and atomically claim the OTP before issuing reset authority.
        var consumeKey = await VerifyOtpAsync(
            FORGOT_PASSWORD_PURPOSE,
            normalizedEmail,
            verifyRequest.Otp,
            cancellationToken);
        var passwordResetToken = CodeGenerationHelper.GenerateOpaqueToken();
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(RESET_SESSION_TTL_MINUTES);
        var resetSessionCreated = false;

        try
        {
            // Step 3: Create the short reset session after this request owns the OTP claim.
            await _cachingService.SetAbsoluteAsync(
                string.Format(RESET_SESSION_KEY_PATTERN, passwordResetToken),
                new ResetSessionCacheResponseDto
                {
                    Email = normalizedEmail,
                    ExpiresAtUtc = expiresAtUtc
                },
                TimeSpan.FromMinutes(RESET_SESSION_TTL_MINUTES),
                cancellationToken);

            resetSessionCreated = true;
        }
        finally
        {
            if (!resetSessionCreated)
            {
                // Step 3a: Release the OTP claim when reset authority was not created so a valid retry remains possible.
                try
                {
                    await _atomicCacheService.RemoveAsync(consumeKey, CancellationToken.None);
                }
                catch (Exception cleanupException)
                {
                    _logger.LogWarning(
                        cleanupException,
                        ApplicationLogConstants.ForgotPasswordLogs.VERIFY_FORGOT_PASSWORD_FLOW_CONSUME_CLEANUP_FAILED);
                }
            }
        }

        // Step 4: Remove obsolete OTP state after reset authority exists; the consume marker still blocks replay.
        var otpKey = string.Format(OTP_KEY_PATTERN, FORGOT_PASSWORD_PURPOSE, normalizedEmail);
        await _cachingService.RemoveAsync(otpKey, cancellationToken);
        await _atomicCacheService.RemoveAsync(
            string.Format(OTP_VERIFY_ATTEMPT_KEY_PATTERN, otpKey),
            cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.ForgotPasswordLogs.VERIFY_FORGOT_PASSWORD_FLOW_COMPLETED);

        return new ResponseDto<PasswordResetTokenResponseDto>(
            new PasswordResetTokenResponseDto
            {
                PasswordResetToken = passwordResetToken,
                ExpiresIn = RESET_SESSION_TTL_MINUTES * 60
            });
    }

    /// <summary>
    /// Verifies the supplied OTP and consumes it when valid.
    /// </summary>
    /// <param name="purpose">The OTP purpose code.</param>
    /// <param name="normalizedEmail">The normalised email address.</param>
    /// <param name="otp">The OTP supplied by the caller.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The atomic consume-marker key owned by the successful request.</returns>
    private async Task<string> VerifyOtpAsync(
        string purpose,
        string normalizedEmail,
        string otp,
        CancellationToken cancellationToken)
    {
        // Step 1: Load the OTP entry; missing state remains indistinguishable from expiry to callers.
        var otpKey = string.Format(OTP_KEY_PATTERN, purpose, normalizedEmail);
        var otpEntry = await _cachingService.GetAsync<OtpCacheResponseDto>(otpKey, cancellationToken);

        if (otpEntry is null || string.IsNullOrWhiteSpace(otpEntry.Code))
        {
            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_INVALID_OR_EXPIRED_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_INVALID);
        }

        // Step 2: Remove stale state so an expired OTP cannot be revived by later writes.
        var remainingTtl = otpEntry.ExpiresAtUtc - DateTime.UtcNow;

        if (remainingTtl <= TimeSpan.Zero)
        {
            await _cachingService.RemoveAsync(otpKey, cancellationToken);
            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_INVALID_OR_EXPIRED_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_INVALID);
        }

        if (otpEntry.Code != otp)
        {
            // Step 3: Count failures atomically so parallel requests cannot lose attempts.
            var attemptKey = string.Format(OTP_VERIFY_ATTEMPT_KEY_PATTERN, otpKey);
            var attempts = await _atomicCacheService.IncrementAsync(
                attemptKey,
                remainingTtl,
                cancellationToken);
            if (attempts >= OTP_MAX_VERIFY_ATTEMPTS)
            {
                await _cachingService.RemoveAsync(otpKey, cancellationToken);
                await _atomicCacheService.RemoveAsync(attemptKey, cancellationToken);
                _logger.LogWarning(ApplicationLogConstants.ForgotPasswordLogs.VERIFY_FORGOT_PASSWORD_FLOW_OTP_LOCKED);
            }

            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_INVALID_OR_EXPIRED_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_INVALID);
        }

        // Step 4: Claim the valid OTP atomically so only one request may create a reset session.
        var consumeKey = string.Format(OTP_CONSUME_KEY_PATTERN, otpKey);
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
