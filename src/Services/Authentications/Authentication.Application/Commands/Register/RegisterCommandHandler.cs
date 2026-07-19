namespace Authentication.Application.Commands.Register;

/// <summary>
/// Handles registration OTP send and resend requests.
/// </summary>
public class RegisterCommandHandler : ICommandHandler<RegisterCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICachingService _cachingService;
    private readonly IAtomicCacheService _atomicCacheService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    /// <summary>
    /// Creates the register OTP handler with registration preparation, cache state, and step logging dependencies.
    /// </summary>
    /// <param name="authenticationService">The service that validates and prepares pending registration payloads.</param>
    /// <param name="cachingService">The cache service used for pending registration session data.</param>
    /// <param name="atomicCacheService">The atomic cache service used for cooldown and request counters.</param>
    /// <param name="logger">The logger used for registration OTP flow tracking.</param>
    public RegisterCommandHandler(
        IAuthenticationService authenticationService,
        ICachingService cachingService,
        IAtomicCacheService atomicCacheService,
        ILogger<RegisterCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _cachingService = cachingService;
        _atomicCacheService = atomicCacheService;
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
        // Mapster owns request normalisation so cache keys and master-data lookups share one stable shape.
        var registerRequest = request.Adapt<RegisterRequestDto>();
        var normalizedEmail = registerRequest.Email;

        // Step 1: Reserve the cooldown atomically so concurrent requests cannot both send an OTP.
        var cooldownKey = string.Format(OTP_COOLDOWN_KEY_PATTERN, REGISTER_PURPOSE, normalizedEmail);
        var reservedCooldown = await _atomicCacheService.TrySetIfAbsentAsync(
            cooldownKey,
            OTP_COOLDOWN_VALUE,
            TimeSpan.FromSeconds(OTP_COOLDOWN_SECONDS),
            cancellationToken);
        if (!reservedCooldown)
        {
            _logger.LogInformation(ApplicationLogConstants.RegisterLogs.REGISTER_FLOW_SKIPPED_COOLDOWN);
            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_COOLDOWN_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_COOLDOWN,
                StatusCodes.Status429TooManyRequests);
        }

        // Count accepted OTP requests before doing heavier user uniqueness and password-hash work.
        await CheckOtpThrottleAsync(REGISTER_PURPOSE, normalizedEmail, REGISTER_OTP_LIMIT, cancellationToken);
        var pendingRegister = await _authenticationService.BuildPendingRegisterAsync(
            registerRequest,
            cancellationToken);
        var otpCode = CodeGenerationHelper.GenerateNumericCode(OTP_LENGTH);
        var otpCacheResponse = new OtpCacheRequestDto
        {
            Purpose = REGISTER_PURPOSE,
            NormalizedEmail = normalizedEmail,
            OtpCode = otpCode
        }.Adapt<OtpCacheResponseDto>();
        otpCacheResponse.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(OTP_TTL_MINUTES);

        var otpTtl = TimeSpan.FromMinutes(OTP_TTL_MINUTES);
        var registerSessionKey = string.Format(REGISTER_SESSION_KEY_PATTERN, normalizedEmail);
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
        // Email delivery is the boundary where cached register session state becomes useful to the user.
        var sent = await _authenticationService.SendOtpEmailAsync(
            new OtpEmailRequestDto
            {
                Email = normalizedEmail,
                Subject = ApplicationMessageConstants.EmailMessages.EMAIL_SUBJECT_VERIFY_ACCOUNT,
                OtpCode = otpCode,
                Purpose = REGISTER_PURPOSE
            },
            cancellationToken);
        if (!sent.IsSuccess)
        {
            // Step 5a: Roll back session and cooldown state when the OTP never reaches the user.
            await _cachingService.RemoveAsync(
                registerSessionKey,
                cancellationToken);
            await _atomicCacheService.RemoveAsync(cooldownKey, cancellationToken);
            _logger.LogWarning(ApplicationLogConstants.RegisterLogs.REGISTER_FLOW_ROLLBACK_OTP_SEND_FAILED);
            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_SEND_FAILED_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_FORBIDDEN_OPERATION,
                StatusCodes.Status503ServiceUnavailable);
        }

        _logger.LogInformation(ApplicationLogConstants.RegisterLogs.REGISTER_FLOW_COMPLETED);

        return new ResponseDto<OperationStatusResponseDto>(
            OperationStatusResponseHelper.Success(ApplicationMessageConstants.OtpMessages.OTP_SENT_MESSAGE));
    }

    /// <summary>
    /// Checks OTP request rate limits for the target purpose and email.
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
        // Increment and initialize the fixed request window as one atomic cache operation.
        var currentLimit = await _atomicCacheService.IncrementAsync(
            string.Format(OTP_LIMIT_KEY_PATTERN, purpose, normalizedEmail),
            TimeSpan.FromMinutes(OTP_LIMIT_TTL_MINUTES),
            cancellationToken);

        if (currentLimit > limit)
        {
            // Reject excess requests without resetting the existing bucket or extending its rate-limit window.
            _logger.LogInformation(ApplicationLogConstants.RegisterLogs.REGISTER_FLOW_SKIPPED_THROTTLED);
            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_RATE_LIMIT_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_RATE_LIMIT,
                StatusCodes.Status429TooManyRequests);
        }

    }

}
