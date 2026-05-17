namespace Authentication.Application.Commands.VerifyRegisterEmail;

/// <summary>
/// Handles registration email OTP verification.
/// </summary>
public class VerifyRegisterEmailCommandHandler : ICommandHandler<VerifyRegisterEmailCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICachingService _cachingService;
    private readonly ILogger<VerifyRegisterEmailCommandHandler> _logger;

    /// <summary>
    /// Creates the register-email verification handler with register session cache and account provisioning services.
    /// </summary>
    /// <param name="authenticationService">The service that completes account creation.</param>
    /// <param name="cachingService">The cache service used for register session state.</param>
    /// <param name="logger">The logger used for register verification flow tracking.</param>
    public VerifyRegisterEmailCommandHandler(
        IAuthenticationService authenticationService,
        ICachingService cachingService,
        ILogger<VerifyRegisterEmailCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _cachingService = cachingService;
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
        // Register session is the source of truth for the latest register/resend payload and OTP state.
        var verifyRequest = request.Adapt<VerifyRegisterEmailRequestDto>();
        var normalizedEmail = verifyRequest.Email;
        var registerSessionKey = AuthenticationFlowHelper.BuildRegisterSessionKey(normalizedEmail);
        var registerSession = await _cachingService.GetAsync<RegisterSessionCacheRequestDto>(
            registerSessionKey,
            cancellationToken);
        if (registerSession?.PendingRegister is null || registerSession.Otp is null)
        {
            throw new ApiException(REGISTRATION_SESSION_EXPIRED_MESSAGE, AUTH_PENDING_REGISTER_NOT_FOUND);
        }

        _logger.LogInformation(VERIFY_REGISTER_FLOW_STEP1_SESSION_LOADED);

        await VerifyOtpAsync(registerSessionKey, registerSession, verifyRequest.Otp, cancellationToken);
        _logger.LogInformation(VERIFY_REGISTER_FLOW_STEP2_OTP_VERIFIED);

        // Account creation re-checks uniqueness and writes Party + User + UserParty atomically.
        var result = await _authenticationService.CompleteRegistrationAsync(
            registerSession.PendingRegister,
            cancellationToken);
        _logger.LogInformation(VERIFY_REGISTER_FLOW_STEP3_ACCOUNT_CREATED);

        // Consume the register session only after the account graph is created successfully.
        await _cachingService.RemoveAsync(registerSessionKey, cancellationToken);
        _logger.LogInformation(VERIFY_REGISTER_FLOW_STEP4_SESSION_REMOVED);
        _logger.LogInformation(VERIFY_REGISTER_FLOW_COMPLETED);

        return new ResponseDto<OperationStatusResponseDto>(result);
    }

    /// <summary>
    /// Verifies the supplied OTP against the cached register session.
    /// </summary>
    /// <param name="registerSessionKey">The cache key that stores the register session.</param>
    /// <param name="registerSession">The cached register session containing pending account data and OTP state.</param>
    /// <param name="otp">The OTP supplied by the caller.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that completes when the OTP is valid.</returns>
    private async Task VerifyOtpAsync(
        string registerSessionKey,
        RegisterSessionCacheRequestDto registerSession,
        string otp,
        CancellationToken cancellationToken)
    {
        var otpEntry = registerSession.Otp;
        if (otpEntry is null || string.IsNullOrWhiteSpace(otpEntry.Code))
        {
            throw new ApiException(OTP_INVALID_OR_EXPIRED_MESSAGE, AUTH_OTP_INVALID);
        }

        var remainingTtl = otpEntry.ExpiresAtUtc - DateTime.UtcNow;
        if (remainingTtl <= TimeSpan.Zero)
        {
            await _cachingService.RemoveAsync(registerSessionKey, cancellationToken);
            throw new ApiException(OTP_INVALID_OR_EXPIRED_MESSAGE, AUTH_OTP_INVALID);
        }

        if (!string.Equals(otpEntry.Code, otp, StringComparison.Ordinal))
        {
            // Persist failed attempts so repeated incorrect OTP values eventually consume the current OTP.
            otpEntry.Attempts++;
            if (otpEntry.Attempts >= OTP_MAX_VERIFY_ATTEMPTS)
            {
                await _cachingService.RemoveAsync(registerSessionKey, cancellationToken);
                _logger.LogInformation(VERIFY_REGISTER_FLOW_OTP_LOCKED);
            }
            else
            {
                await _cachingService.SetAbsoluteAsync(
                    registerSessionKey,
                    registerSession,
                    remainingTtl,
                    cancellationToken);
                _logger.LogInformation(VERIFY_REGISTER_FLOW_OTP_ATTEMPT_RECORDED);
            }

            throw new ApiException(OTP_INVALID_OR_EXPIRED_MESSAGE, AUTH_OTP_INVALID);
        }
    }
}
