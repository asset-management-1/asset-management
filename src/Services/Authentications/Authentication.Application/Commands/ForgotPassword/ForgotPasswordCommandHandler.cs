namespace Authentication.Application.Commands.ForgotPassword;

/// <summary>
/// Handles forgot-password OTP send requests.
/// </summary>
public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICachingService _cachingService;
    private readonly IAtomicCacheService _atomicCacheService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    /// <summary>
    /// Creates the forgot-password OTP handler with account lookup, OTP cache, and enumeration-safe flow logging dependencies.
    /// </summary>
    /// <param name="authenticationService">The service that checks user existence and sends OTP emails.</param>
    /// <param name="cachingService">The cache service used for OTP payload state.</param>
    /// <param name="atomicCacheService">The atomic cache service used for cooldown and request counters.</param>
    /// <param name="logger">The logger used for generic forgot-password flow tracking.</param>
    public ForgotPasswordCommandHandler(
        IAuthenticationService authenticationService,
        ICachingService cachingService,
        IAtomicCacheService atomicCacheService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _cachingService = cachingService;
        _atomicCacheService = atomicCacheService;
        _logger = logger;
    }

    /// <summary>
    /// Starts the enumeration-safe forgot-password flow and sends a reset OTP when the account exists.
    /// </summary>
    /// <param name="request">The forgot-password command payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the generic forgot-password message.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Mapster normalises email first so every cache key for this flow uses the same namespace.
        var forgotPasswordRequest = request.Adapt<ForgotPasswordRequestDto>();
        var normalizedEmail = forgotPasswordRequest.Email;

        try
        {
            // Throttle failures intentionally keep the same public response to avoid account enumeration signals.
            await CheckOtpThrottleAsync(FORGOT_PASSWORD_PURPOSE, normalizedEmail, FORGOT_PASSWORD_OTP_LIMIT, cancellationToken);
        }
        catch (ApiException ex)
        {
            _logger.LogInformation(ex, ApplicationLogConstants.ForgotPasswordLogs.FORGOT_PASSWORD_FLOW_SKIPPED_THROTTLED);
            return new ResponseDto<OperationStatusResponseDto>(
                OperationStatusResponseHelper.Success(ApplicationMessageConstants.OtpMessages.FORGOT_PASSWORD_SUCCESS_MESSAGE));
        }

        // Missing accounts follow the same response contract but only store cooldown state.
        var userExists = await _authenticationService.UserExistsByEmailAsync(normalizedEmail, cancellationToken);

        if (!userExists)
        {
            // Keep forgot-password enumeration-safe while the reserved cooldown limits repeated probes.
            _logger.LogInformation(ApplicationLogConstants.ForgotPasswordLogs.FORGOT_PASSWORD_FLOW_MISSING_ACCOUNT_GENERIC_RESPONSE);
            return new ResponseDto<OperationStatusResponseDto>(
                OperationStatusResponseHelper.Success(ApplicationMessageConstants.OtpMessages.FORGOT_PASSWORD_SUCCESS_MESSAGE));
        }

        // A valid account gets a fresh OTP entry; resend overwrites the previous code on the same key.
        var otpCode = CodeGenerationHelper.GenerateNumericCode(OTP_LENGTH);
        var otpCacheResponse = new OtpCacheRequestDto
        {
            Purpose = FORGOT_PASSWORD_PURPOSE,
            NormalizedEmail = normalizedEmail,
            OtpCode = otpCode
        }.Adapt<OtpCacheResponseDto>();
        otpCacheResponse.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(OTP_TTL_MINUTES);

        var otpKey = string.Format(OTP_KEY_PATTERN, FORGOT_PASSWORD_PURPOSE, normalizedEmail);

        await _cachingService.SetAbsoluteAsync(
            otpKey,
            otpCacheResponse,
            TimeSpan.FromMinutes(OTP_TTL_MINUTES),
            cancellationToken);
        // Delivery success is required before writing cooldown because failed sends should be retryable.
        var sent = await _authenticationService.SendOtpEmailAsync(
            new OtpEmailRequestDto
            {
                Email = normalizedEmail,
                Subject = ApplicationMessageConstants.EmailMessages.EMAIL_SUBJECT_RESET_PASSWORD,
                OtpCode = otpCode,
                Purpose = FORGOT_PASSWORD_PURPOSE
            },
            cancellationToken);
        if (sent.IsSuccess)
        {
        }
        else
        {
            // Remove the generated OTP when delivery fails so no undelivered reset code remains valid.
            await _cachingService.RemoveAsync(otpKey, cancellationToken);
            await _atomicCacheService.RemoveAsync(
                string.Format(OTP_COOLDOWN_KEY_PATTERN, FORGOT_PASSWORD_PURPOSE, normalizedEmail),
                cancellationToken);
            _logger.LogWarning(ApplicationLogConstants.ForgotPasswordLogs.FORGOT_PASSWORD_FLOW_ROLLBACK_OTP_SEND_FAILED);
        }

        _logger.LogInformation(ApplicationLogConstants.ForgotPasswordLogs.FORGOT_PASSWORD_FLOW_COMPLETED_GENERIC);
        return new ResponseDto<OperationStatusResponseDto>(
            OperationStatusResponseHelper.Success(ApplicationMessageConstants.OtpMessages.FORGOT_PASSWORD_SUCCESS_MESSAGE));
    }

    /// <summary>
    /// Checks OTP cooldown and request-count limits.
    /// </summary>
    /// <param name="purpose">The OTP purpose code.</param>
    /// <param name="normalizedEmail">The normalised email address.</param>
    /// <param name="limit">The maximum number of OTP requests allowed in the rate-limit window.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that completes when the current OTP request is accepted and counted.</returns>
    private async Task CheckOtpThrottleAsync(
        string purpose,
        string normalizedEmail,
        int limit,
        CancellationToken cancellationToken)
    {
        // Reserve the cooldown atomically so concurrent requests cannot both pass the guard.
        var reservedCooldown = await _atomicCacheService.TrySetIfAbsentAsync(
            string.Format(OTP_COOLDOWN_KEY_PATTERN, purpose, normalizedEmail),
            OTP_COOLDOWN_VALUE,
            TimeSpan.FromSeconds(OTP_COOLDOWN_SECONDS),
            cancellationToken);

        if (!reservedCooldown)
        {
            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_COOLDOWN_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_COOLDOWN,
                StatusCodes.Status429TooManyRequests);
        }

        // Increment and initialize the longer request window atomically.
        var currentLimit = await _atomicCacheService.IncrementAsync(
            string.Format(OTP_LIMIT_KEY_PATTERN, purpose, normalizedEmail),
            TimeSpan.FromMinutes(OTP_LIMIT_TTL_MINUTES),
            cancellationToken);
        if (currentLimit > limit)
        {
            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_RATE_LIMIT_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_RATE_LIMIT,
                StatusCodes.Status429TooManyRequests);
        }

    }
}
