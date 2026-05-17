namespace Authentication.Application.Commands.Register;

/// <summary>
/// Handles registration OTP send and resend requests.
/// </summary>
public class RegisterCommandHandler : ICommandHandler<RegisterCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICachingService _cachingService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    /// <summary>
    /// Creates the register OTP handler with registration preparation, cache state, and step logging dependencies.
    /// </summary>
    /// <param name="authenticationService">The service that validates and prepares pending registration payloads.</param>
    /// <param name="cachingService">The cache service used for register session, cooldown, and rate-limit state.</param>
    /// <param name="logger">The logger used for registration OTP flow tracking.</param>
    public RegisterCommandHandler(
        IAuthenticationService authenticationService,
        ICachingService cachingService,
        ILogger<RegisterCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _cachingService = cachingService;
        _logger = logger;
    }

    /// <summary>
    /// Handles user registration and returns OTP send result.
    /// </summary>
    /// <param name="request">The register command payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the OTP-send success message.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // Mapster owns request normalization so cache keys and master-data lookups share one stable shape.
        var registerRequest = request.Adapt<RegisterRequestDto>();
        var normalizedEmail = registerRequest.Email;

        // Reject resend while the register OTP cooldown marker still exists for this email.
        var hasOtpCooldown = !string.IsNullOrWhiteSpace(await _cachingService.GetAsync<string>(
            AuthenticationFlowHelper.BuildOtpCooldownKey(REGISTER_PURPOSE, normalizedEmail),
            cancellationToken));
        if (hasOtpCooldown)
        {
            _logger.LogInformation(REGISTER_FLOW_SKIPPED_COOLDOWN);
            throw new ApiException(
                OTP_COOLDOWN_MESSAGE,
                AUTH_OTP_COOLDOWN,
                StatusCodes.Status429TooManyRequests);
        }

        _logger.LogInformation(REGISTER_FLOW_STEP1_REQUEST_NORMALIZED);

        // Count accepted OTP requests before doing heavier user uniqueness and password-hash work.
        await CheckOtpThrottleAsync(REGISTER_PURPOSE, normalizedEmail, REGISTER_OTP_LIMIT, cancellationToken);
        _logger.LogInformation(REGISTER_FLOW_STEP2_THROTTLE_ACCEPTED);

        var pendingRegister = await _authenticationService.BuildPendingRegisterAsync(
            registerRequest,
            cancellationToken);
        _logger.LogInformation(REGISTER_FLOW_STEP3_PENDING_BUILT);

        var otpCode = AuthenticationFlowHelper.GenerateOtp();
        var otpCacheResponse = new OtpCacheRequestDto
        {
            Purpose = REGISTER_PURPOSE,
            NormalizedEmail = normalizedEmail,
            OtpCode = otpCode
        }.Adapt<OtpCacheResponseDto>();
        otpCacheResponse.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(OTP_TTL_MINUTES);

        var otpTtl = TimeSpan.FromMinutes(OTP_TTL_MINUTES);
        var registerSessionKey = AuthenticationFlowHelper.BuildRegisterSessionKey(normalizedEmail);
        var registerSession = new RegisterSessionCacheRequestDto
        {
            PendingRegister = pendingRegister,
            Otp = otpCacheResponse
        };

        // Resend after cooldown intentionally overwrites the register session so only the newest OTP remains valid.
        await _cachingService.SetAbsoluteAsync(
            registerSessionKey,
            registerSession,
            otpTtl,
            cancellationToken);
        _logger.LogInformation(REGISTER_FLOW_STEP4_SESSION_CACHED);

        // Email delivery is the boundary where cached register session state becomes useful to the user.
        var sent = await _authenticationService.SendOtpEmailAsync(
            new OtpEmailRequestDto
            {
                Email = normalizedEmail,
                Subject = EMAIL_SUBJECT_VERIFY_ACCOUNT,
                OtpCode = otpCode,
                Purpose = REGISTER_PURPOSE
            },
            cancellationToken);
        if (!sent.IsSuccess)
        {
            // Roll back the register session when the OTP never reaches the user.
            await _cachingService.RemoveAsync(
                registerSessionKey,
                cancellationToken);
            _logger.LogWarning(REGISTER_FLOW_ROLLBACK_OTP_SEND_FAILED);
            throw new ApiException(
                OTP_SEND_FAILED_MESSAGE,
                AUTH_FORBIDDEN_OPERATION,
                StatusCodes.Status503ServiceUnavailable);
        }

        _logger.LogInformation(REGISTER_FLOW_STEP5_OTP_SENT);

        // Cooldown is stored only after successful delivery so failed sends can be retried immediately.
        await _cachingService.SetAbsoluteAsync(
            AuthenticationFlowHelper.BuildOtpCooldownKey(REGISTER_PURPOSE, normalizedEmail),
            OTP_COOLDOWN_VALUE,
            TimeSpan.FromSeconds(OTP_COOLDOWN_SECONDS),
            cancellationToken);
        _logger.LogInformation(REGISTER_FLOW_COMPLETED);

        return new ResponseDto<OperationStatusResponseDto>(
            OperationStatusResponseHelper.Success(OTP_SENT_MESSAGE));
    }

    /// <summary>
    /// Checks OTP request rate limits for the target purpose and email.
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
        var currentLimit = await _cachingService.GetAsync<int?>(
            AuthenticationFlowHelper.BuildOtpLimitKey(purpose, normalizedEmail),
            cancellationToken) ?? 0;
        currentLimit++;
        if (currentLimit > limit)
        {
            _logger.LogInformation(REGISTER_FLOW_SKIPPED_THROTTLED);
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
