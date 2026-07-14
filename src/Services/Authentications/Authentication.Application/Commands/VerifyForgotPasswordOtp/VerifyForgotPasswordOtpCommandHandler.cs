namespace Authentication.Application.Commands.VerifyForgotPasswordOtp;

/// <summary>
/// Handles forgot-password OTP verification requests.
/// </summary>
public class VerifyForgotPasswordOtpCommandHandler : ICommandHandler<VerifyForgotPasswordOtpCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly ICachingService _cachingService;
    private readonly ILogger<VerifyForgotPasswordOtpCommandHandler> _logger;

    /// <summary>
    /// Creates the forgot-password OTP verification handler with OTP cache and reset-session cache services.
    /// </summary>
    /// <param name="cachingService">The cache service used for OTP and reset-session state.</param>
    /// <param name="logger">The logger used for OTP verification flow tracking.</param>
    public VerifyForgotPasswordOtpCommandHandler(
        ICachingService cachingService,
        ILogger<VerifyForgotPasswordOtpCommandHandler> logger)
    {
        _cachingService = cachingService;
        _logger = logger;
    }

    /// <summary>
    /// Handles forgot-password OTP verification and opens a reset session.
    /// </summary>
    /// <param name="request">The forgot-password OTP verification payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the OTP-verification success message.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        VerifyForgotPasswordOtpCommand request,
        CancellationToken cancellationToken)
    {
        // Mapster normalizes the email before OTP and reset-session keys are resolved.
        var verifyRequest = request.Adapt<VerifyForgotPasswordOtpRequestDto>();
        var normalizedEmail = verifyRequest.Email;

        _logger.LogInformation(ApplicationLogConstants.ForgotPasswordLogs.VERIFY_FORGOT_PASSWORD_FLOW_STEP1_REQUEST_NORMALIZED);

        await VerifyOtpAsync(FORGOT_PASSWORD_PURPOSE, normalizedEmail, verifyRequest.Otp, cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.ForgotPasswordLogs.VERIFY_FORGOT_PASSWORD_FLOW_STEP2_OTP_VERIFIED);

        // A short reset session lets the password-change endpoint proceed without keeping the OTP valid.
        await _cachingService.SetAbsoluteAsync(
            AuthenticationFlowHelper.BuildResetSessionKey(normalizedEmail),
            new ResetSessionCacheResponseDto { CreatedAt = DateTime.UtcNow },
            TimeSpan.FromMinutes(RESET_SESSION_TTL_MINUTES),
            cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.ForgotPasswordLogs.VERIFY_FORGOT_PASSWORD_FLOW_STEP3_RESET_SESSION_CREATED);

        return new ResponseDto<OperationStatusResponseDto>(
            OperationStatusResponseHelper.Success(ApplicationMessageConstants.OtpMessages.OTP_VERIFIED_SUCCESS_MESSAGE));
    }

    /// <summary>
    /// Verifies the supplied OTP and consumes it when valid.
    /// </summary>
    /// <param name="purpose">The OTP purpose code.</param>
    /// <param name="normalizedEmail">The normalized email address.</param>
    /// <param name="otp">The OTP supplied by the caller.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that completes when the OTP is valid and consumed.</returns>
    private async Task VerifyOtpAsync(
        string purpose,
        string normalizedEmail,
        string otp,
        CancellationToken cancellationToken)
    {
        // Load the one-time OTP entry; missing cache state is indistinguishable from expiry to callers.
        var otpKey = AuthenticationFlowHelper.BuildOtpKey(purpose, normalizedEmail);
        var otpEntry = await _cachingService.GetAsync<OtpCacheResponseDto>(otpKey, cancellationToken);

        if (otpEntry is null || string.IsNullOrWhiteSpace(otpEntry.Code))
        {
            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_INVALID_OR_EXPIRED_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_INVALID);
        }

        // Remove stale entries explicitly so expired credentials cannot be revived by later writes.
        var remainingTtl = otpEntry.ExpiresAtUtc - DateTime.UtcNow;

        if (remainingTtl <= TimeSpan.Zero)
        {
            await _cachingService.RemoveAsync(otpKey, cancellationToken);
            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_INVALID_OR_EXPIRED_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_INVALID);
        }

        if (!string.Equals(otpEntry.Code, otp, StringComparison.Ordinal))
        {
            // Persist remaining attempts until the OTP is consumed or locked out.
            otpEntry.Attempts++;
            if (otpEntry.Attempts >= OTP_MAX_VERIFY_ATTEMPTS)
            {
                await _cachingService.RemoveAsync(otpKey, cancellationToken);
                _logger.LogInformation(ApplicationLogConstants.ForgotPasswordLogs.VERIFY_FORGOT_PASSWORD_FLOW_OTP_LOCKED);
            }
            else
            {
                await _cachingService.SetAbsoluteAsync(otpKey, otpEntry, remainingTtl, cancellationToken);
                _logger.LogInformation(ApplicationLogConstants.ForgotPasswordLogs.VERIFY_FORGOT_PASSWORD_FLOW_OTP_ATTEMPT_RECORDED);
            }

            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_INVALID_OR_EXPIRED_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_INVALID);
        }

        // Successful verification consumes the OTP before a reset session can be issued.
        await _cachingService.RemoveAsync(otpKey, cancellationToken);
    }
}
