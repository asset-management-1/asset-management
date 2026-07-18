namespace Authentication.Application.Commands.VerifyRegisterEmail;

/// <summary>
/// Handles registration email OTP verification.
/// </summary>
public class VerifyRegisterEmailCommandHandler : ICommandHandler<VerifyRegisterEmailCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICachingService _cachingService;
    private readonly IAtomicCacheService _atomicCacheService;
    private readonly ILogger<VerifyRegisterEmailCommandHandler> _logger;

    /// <summary>
    /// Creates the register-email verification handler with register session cache and account provisioning services.
    /// </summary>
    /// <param name="authenticationService">The service that completes account creation.</param>
    /// <param name="cachingService">The cache service used for register session state.</param>
    /// <param name="atomicCacheService">The atomic cache service used for attempts and one-time OTP consumption.</param>
    /// <param name="logger">The logger used for register verification flow tracking.</param>
    public VerifyRegisterEmailCommandHandler(
        IAuthenticationService authenticationService,
        ICachingService cachingService,
        IAtomicCacheService atomicCacheService,
        ILogger<VerifyRegisterEmailCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _cachingService = cachingService;
        _atomicCacheService = atomicCacheService;
        _logger = logger;
    }

    /// <summary>
    /// Handles register-email verification and completes account creation.
    /// </summary>
    /// <param name="request">The register-email verification payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the registration-complete success message.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        VerifyRegisterEmailCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load the latest registration session before accepting any OTP state from the request.
        var verifyRequest = request.Adapt<VerifyRegisterEmailRequestDto>();
        var normalizedEmail = verifyRequest.Email;
        var registerSessionKey = string.Format(REGISTER_SESSION_KEY_PATTERN, normalizedEmail);
        var registerSession = await _cachingService.GetAsync<RegisterSessionCacheRequestDto>(
            registerSessionKey,
            cancellationToken);
        if (registerSession?.PendingRegister is null || registerSession.Otp is null)
        {
            throw new ApiException(ApplicationErrorConstants.OtpErrors.REGISTRATION_SESSION_EXPIRED_MESSAGE, ApplicationErrorConstants.OtpErrorCodes.AUTH_PENDING_REGISTER_NOT_FOUND);
        }

        _logger.LogInformation(ApplicationLogConstants.RegisterLogs.VERIFY_REGISTER_FLOW_STEP1_SESSION_LOADED);

        // Step 2: Verify and atomically claim the OTP before account creation can start.
        var consumeKey = await VerifyOtpAsync(
            registerSessionKey,
            registerSession,
            verifyRequest.Otp,
            cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.RegisterLogs.VERIFY_REGISTER_FLOW_STEP2_OTP_VERIFIED);

        OperationStatusResponseDto result;
        var registrationCompleted = false;

        try
        {
            // Step 3: Recheck uniqueness and write User, Party, and UserParty atomically.
            result = await _authenticationService.CompleteRegistrationAsync(
                registerSession.PendingRegister,
                cancellationToken);

            registrationCompleted = true;
        }
        finally
        {
            if (!registrationCompleted)
            {
                // Step 3a: Release the OTP claim after an unsuccessful account write so the caller may retry safely.
                try
                {
                    await _atomicCacheService.RemoveAsync(consumeKey, CancellationToken.None);
                }
                catch (Exception cleanupException)
                {
                    _logger.LogWarning(
                        cleanupException,
                        ApplicationLogConstants.RegisterLogs.VERIFY_REGISTER_FLOW_CONSUME_CLEANUP_FAILED);
                }
            }
        }

        _logger.LogInformation(ApplicationLogConstants.RegisterLogs.VERIFY_REGISTER_FLOW_STEP3_ACCOUNT_CREATED);

        // Step 4: Remove pending registration state only after the account graph commits.
        await _cachingService.RemoveAsync(registerSessionKey, cancellationToken);
        await _atomicCacheService.RemoveAsync(
            string.Format(OTP_VERIFY_ATTEMPT_KEY_PATTERN, registerSessionKey),
            cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.RegisterLogs.VERIFY_REGISTER_FLOW_STEP4_SESSION_REMOVED);
        _logger.LogInformation(ApplicationLogConstants.RegisterLogs.VERIFY_REGISTER_FLOW_COMPLETED);

        return new ResponseDto<OperationStatusResponseDto>(result);
    }

    /// <summary>
    /// Verifies the supplied OTP against the cached register session.
    /// </summary>
    /// <param name="registerSessionKey">The cache key that stores the register session.</param>
    /// <param name="registerSession">The cached register session containing pending account data and OTP state.</param>
    /// <param name="otp">The OTP supplied by the caller.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The atomic consume-marker key owned by the successful request.</returns>
    private async Task<string> VerifyOtpAsync(
        string registerSessionKey,
        RegisterSessionCacheRequestDto registerSession,
        string otp,
        CancellationToken cancellationToken)
    {
        // Step 1: Read the OTP state owned by the registration session.
        var otpEntry = registerSession.Otp;

        if (otpEntry is null || string.IsNullOrWhiteSpace(otpEntry.Code))
        {
            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_INVALID_OR_EXPIRED_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_INVALID);
        }

        // Step 2: Remove expired registration state before rejecting the request.
        var remainingTtl = otpEntry.ExpiresAtUtc - DateTime.UtcNow;

        if (remainingTtl <= TimeSpan.Zero)
        {
            await _cachingService.RemoveAsync(registerSessionKey, cancellationToken);
            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_INVALID_OR_EXPIRED_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_INVALID);
        }

        if (otpEntry.Code != otp)
        {
            // Step 3: Count failed attempts atomically so concurrent requests cannot bypass the limit.
            var attemptKey = string.Format(OTP_VERIFY_ATTEMPT_KEY_PATTERN, registerSessionKey);
            var attempts = await _atomicCacheService.IncrementAsync(
                attemptKey,
                remainingTtl,
                cancellationToken);
            if (attempts >= OTP_MAX_VERIFY_ATTEMPTS)
            {
                await _cachingService.RemoveAsync(registerSessionKey, cancellationToken);
                await _atomicCacheService.RemoveAsync(attemptKey, cancellationToken);
                _logger.LogInformation(ApplicationLogConstants.RegisterLogs.VERIFY_REGISTER_FLOW_OTP_LOCKED);
            }
            else
            {
                _logger.LogInformation(
                    ApplicationLogConstants.RegisterLogs.VERIFY_REGISTER_FLOW_OTP_ATTEMPT_RECORDED);
            }

            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_INVALID_OR_EXPIRED_MESSAGE,
                ApplicationErrorConstants.OtpErrorCodes.AUTH_OTP_INVALID);
        }

        // Step 4: Claim the valid OTP atomically so only one request may complete registration.
        var consumeKey = string.Format(OTP_CONSUME_KEY_PATTERN, registerSessionKey);
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
