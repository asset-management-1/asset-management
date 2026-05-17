namespace Authentication.Application.Commands.ForgotPassword;

/// <summary>
/// Handles forgot-password OTP send requests.
/// </summary>
public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICachingService _cachingService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    /// <summary>
    /// Creates the forgot-password OTP handler with account lookup, OTP cache, and enumeration-safe flow logging dependencies.
    /// </summary>
    /// <param name="authenticationService">The service that checks user existence and sends OTP emails.</param>
    /// <param name="cachingService">The cache service used for OTP throttle and payload state.</param>
    /// <param name="logger">The logger used for generic forgot-password flow tracking.</param>
    public ForgotPasswordCommandHandler(
        IAuthenticationService authenticationService,
        ICachingService cachingService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _cachingService = cachingService;
        _logger = logger;
    }

    /// <summary>
    /// Handles forgot-password flow by resetting password for the matched user account.
    /// </summary>
    /// <param name="request">The forgot-password command payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the generic forgot-password message.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Mapster normalizes email first so every cache key for this flow uses the same namespace.
        var forgotPasswordRequest = request.Adapt<ForgotPasswordRequestDto>();
        var normalizedEmail = forgotPasswordRequest.Email;
        _logger.LogInformation(FORGOT_PASSWORD_FLOW_STEP1_REQUEST_NORMALIZED);

        try
        {
            // Throttle failures intentionally keep the same public response to avoid account enumeration signals.
            await CheckOtpThrottleAsync(FORGOT_PASSWORD_PURPOSE, normalizedEmail, FORGOT_PASSWORD_OTP_LIMIT, cancellationToken);
            _logger.LogInformation(FORGOT_PASSWORD_FLOW_STEP2_THROTTLE_ACCEPTED);
        }
        catch (ApiException ex)
        {
            _logger.LogInformation(ex, FORGOT_PASSWORD_FLOW_SKIPPED_THROTTLED);
            return new ResponseDto<OperationStatusResponseDto>(
                OperationStatusResponseHelper.Success(FORGOT_PASSWORD_SUCCESS_MESSAGE));
        }

        // Missing accounts follow the same response contract but only store cooldown state.
        var userExists = await _authenticationService.UserExistsByEmailAsync(normalizedEmail, cancellationToken);
        _logger.LogInformation(FORGOT_PASSWORD_FLOW_STEP3_ACCOUNT_LOOKUP_COMPLETED);

        if (!userExists)
        {
            // Keep forgot-password enumeration-safe: callers always receive the same public success response.
            await _cachingService.SetAbsoluteAsync(
                AuthenticationFlowHelper.BuildOtpCooldownKey(FORGOT_PASSWORD_PURPOSE, normalizedEmail),
                OTP_COOLDOWN_VALUE,
                TimeSpan.FromSeconds(OTP_COOLDOWN_SECONDS),
                cancellationToken);

            _logger.LogInformation(FORGOT_PASSWORD_FLOW_MISSING_ACCOUNT_GENERIC_RESPONSE);
            return new ResponseDto<OperationStatusResponseDto>(
                OperationStatusResponseHelper.Success(FORGOT_PASSWORD_SUCCESS_MESSAGE));
        }

        // A valid account gets a fresh OTP entry; resend overwrites the previous code on the same key.
        var otpCode = AuthenticationFlowHelper.GenerateOtp();
        var otpCacheResponse = new OtpCacheRequestDto
        {
            Purpose = FORGOT_PASSWORD_PURPOSE,
            NormalizedEmail = normalizedEmail,
            OtpCode = otpCode
        }.Adapt<OtpCacheResponseDto>();
        otpCacheResponse.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(OTP_TTL_MINUTES);

        var otpKey = AuthenticationFlowHelper.BuildOtpKey(FORGOT_PASSWORD_PURPOSE, normalizedEmail);
        await _cachingService.SetAbsoluteAsync(
            otpKey,
            otpCacheResponse,
            TimeSpan.FromMinutes(OTP_TTL_MINUTES),
            cancellationToken);
        _logger.LogInformation(FORGOT_PASSWORD_FLOW_STEP4_OTP_CACHED);

        // Delivery success is required before writing cooldown because failed sends should be retryable.
        var sent = await _authenticationService.SendOtpEmailAsync(
            new OtpEmailRequestDto
            {
                Email = normalizedEmail,
                Subject = EMAIL_SUBJECT_RESET_PASSWORD,
                OtpCode = otpCode,
                Purpose = FORGOT_PASSWORD_PURPOSE
            },
            cancellationToken);
        if (sent.IsSuccess)
        {
            await _cachingService.SetAbsoluteAsync(
                AuthenticationFlowHelper.BuildOtpCooldownKey(FORGOT_PASSWORD_PURPOSE, normalizedEmail),
                OTP_COOLDOWN_VALUE,
                TimeSpan.FromSeconds(OTP_COOLDOWN_SECONDS),
                cancellationToken);
            _logger.LogInformation(FORGOT_PASSWORD_FLOW_STEP5_OTP_SENT);
        }
        else
        {
            // Remove the generated OTP when delivery fails so no undelivered reset code remains valid.
            await _cachingService.RemoveAsync(otpKey, cancellationToken);
            _logger.LogWarning(FORGOT_PASSWORD_FLOW_ROLLBACK_OTP_SEND_FAILED);
        }

        _logger.LogInformation(FORGOT_PASSWORD_FLOW_COMPLETED_GENERIC);
        return new ResponseDto<OperationStatusResponseDto>(
            OperationStatusResponseHelper.Success(FORGOT_PASSWORD_SUCCESS_MESSAGE));
    }

    /// <summary>
    /// Checks OTP cooldown and request-count limits.
    /// </summary>
    /// <param name="purpose">The OTP purpose code.</param>
    /// <param name="normalizedEmail">The normalized email address.</param>
    /// <param name="limit">The maximum number of OTP requests allowed in the rate-limit window.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that completes when the current OTP request is accepted and counted.</returns>
    private async Task CheckOtpThrottleAsync(
        string purpose,
        string normalizedEmail,
        int limit,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(await _cachingService.GetAsync<string>(
            AuthenticationFlowHelper.BuildOtpCooldownKey(purpose, normalizedEmail),
            cancellationToken)))
        {
            throw new ApiException(
                OTP_COOLDOWN_MESSAGE,
                AUTH_OTP_COOLDOWN,
                StatusCodes.Status429TooManyRequests);
        }

        var currentLimit = await _cachingService.GetAsync<int?>(
            AuthenticationFlowHelper.BuildOtpLimitKey(purpose, normalizedEmail),
            cancellationToken) ?? 0;
        currentLimit++;
        if (currentLimit > limit)
        {
            throw new ApiException(
                OTP_RATE_LIMIT_MESSAGE,
                AUTH_OTP_RATE_LIMIT,
                StatusCodes.Status429TooManyRequests);
        }

        await _cachingService.SetAbsoluteAsync(
            AuthenticationFlowHelper.BuildOtpLimitKey(purpose, normalizedEmail),
            currentLimit,
            TimeSpan.FromMinutes(OTP_LIMIT_TTL_MINUTES),
            cancellationToken);
    }
}
