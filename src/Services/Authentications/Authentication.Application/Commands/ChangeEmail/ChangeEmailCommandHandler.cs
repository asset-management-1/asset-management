namespace Authentication.Application.Commands.ChangeEmail;

/// <summary>
/// Handles change-email OTP send and resend requests.
/// </summary>
public class ChangeEmailCommandHandler : ICommandHandler<ChangeEmailCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authenticationService;
    private readonly ICachingService _cachingService;
    private readonly IAtomicCacheService _atomicCacheService;
    private readonly IAuthService _authService;
    private readonly ILogger<ChangeEmailCommandHandler> _logger;

    /// <summary>
    /// Creates the change-email OTP handler with current-user validation, cache state, email delivery, and flow logging dependencies.
    /// </summary>
    /// <param name="userService">The service that validates current-user email changes.</param>
    /// <param name="authenticationService">The service that sends auth-related emails.</param>
    /// <param name="cachingService">The cache service used for change-email session state.</param>
    /// <param name="atomicCacheService">The atomic cache service used for cooldown reservation.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    /// <param name="logger">The logger used for non-blocking old-email notification failures.</param>
    public ChangeEmailCommandHandler(
        IUserService userService,
        IAuthenticationService authenticationService,
        ICachingService cachingService,
        IAtomicCacheService atomicCacheService,
        IAuthService authService,
        ILogger<ChangeEmailCommandHandler> logger)
    {
        _userService = userService;
        _authenticationService = authenticationService;
        _cachingService = cachingService;
        _atomicCacheService = atomicCacheService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Starts or resends change-email verification for the current authenticated user.
    /// </summary>
    /// <param name="request">The change-email command payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the OTP-send success message.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        ChangeEmailCommand request,
        CancellationToken cancellationToken)
    {
        // Current user and cooldown are checked before preparing any change-email session state.
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);

        // Step 1: Reserve the cooldown before preparing or sending concurrent change-email requests.
        var cooldownKey = string.Format(CHANGE_EMAIL_COOLDOWN_KEY_PATTERN, currentUserPublicId);
        var reservedCooldown = await _atomicCacheService.TrySetIfAbsentAsync(
            cooldownKey,
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

        // UserService validates ownership rules and normalises the target email for the session cache below.
        var changeEmail = await _userService.PrepareChangeEmailAsync(
            request.Adapt<ChangeEmailRequestDto>(),
            cancellationToken);
        var otpCode = CodeGenerationHelper.GenerateNumericCode(OTP_LENGTH);
        var otpCacheResponse = new OtpCacheRequestDto
        {
            Purpose = CHANGE_EMAIL_PURPOSE,
            NormalizedEmail = changeEmail.NewEmail,
            OtpCode = otpCode
        }.Adapt<OtpCacheResponseDto>();
        otpCacheResponse.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(OTP_TTL_MINUTES);

        var otpTtl = TimeSpan.FromMinutes(OTP_TTL_MINUTES);
        var changeEmailSessionKey = string.Format(CHANGE_EMAIL_SESSION_KEY_PATTERN, currentUserPublicId);
        var changeEmailSession = new ChangeEmailSessionCacheResponseDto
        {
            NewEmail = changeEmail.NewEmail,
            Otp = otpCacheResponse
        };

        // Overwrite the change-email session so resend or target-email changes invalidate older OTP values immediately.
        await _cachingService.SetAbsoluteAsync(
            changeEmailSessionKey,
            changeEmailSession,
            otpTtl,
            cancellationToken);
        // The new email must receive the OTP before cooldown/security notification is recorded.
        var sent = await _authenticationService.SendOtpEmailAsync(
            new OtpEmailRequestDto
            {
                Email = changeEmail.NewEmail,
                Subject = ApplicationMessageConstants.EmailMessages.EMAIL_SUBJECT_CHANGE_EMAIL,
                OtpCode = otpCode,
                Purpose = CHANGE_EMAIL_PURPOSE
            },
            cancellationToken);
        if (!sent.IsSuccess)
        {
            // Remove the combined session because the OTP was never delivered.
            await _cachingService.RemoveAsync(
                changeEmailSessionKey,
                cancellationToken);
            await _atomicCacheService.RemoveAsync(cooldownKey, cancellationToken);
            _logger.LogWarning(ApplicationLogConstants.ChangeEmailLogs.CHANGE_EMAIL_FLOW_ROLLBACK_OTP_SEND_FAILED);
            throw new ApiException(
                ApplicationErrorConstants.OtpErrors.OTP_SEND_FAILED_MESSAGE,
                ApplicationErrorConstants.AccountErrorCodes.AUTH_FORBIDDEN_OPERATION,
                StatusCodes.Status503ServiceUnavailable);
        }

        // Old-email notification is best-effort and must not block ownership verification of the new email.
        await TrySendSecurityNotificationAsync(changeEmail, currentUserPublicId, cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.ChangeEmailLogs.CHANGE_EMAIL_FLOW_COMPLETED, currentUserPublicId);

        return new ResponseDto<OperationStatusResponseDto>(
            OperationStatusResponseHelper.Success(ApplicationMessageConstants.ProfileMessages.CHANGE_EMAIL_OTP_SENT_MESSAGE));
    }

    /// <summary>
    /// Sends the old-email notification without blocking the new-email OTP flow.
    /// </summary>
    /// <param name="changeEmail">The prepared change-email state.</param>
    /// <param name="currentUserPublicId">The current user's public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the notification send.</param>
    /// <returns>A task that completes after the best-effort notification attempt.</returns>
    private async Task TrySendSecurityNotificationAsync(
        ChangeEmailStartResponseDto changeEmail,
        Guid currentUserPublicId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Notify the previous address after the new OTP flow starts; this alert must not block the requested change.
            var sent = await _authenticationService.SendChangeEmailSecurityNotificationAsync(
                changeEmail.Adapt<ChangeEmailSecurityNotificationRequestDto>(),
                cancellationToken);

            if (!sent.IsSuccess)
            {
                _logger.LogWarning(ApplicationLogConstants.ChangeEmailLogs.CHANGE_EMAIL_SECURITY_NOTIFICATION_FAILED, currentUserPublicId);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Keep the main OTP flow successful when the secondary security notification provider is unavailable.
            _logger.LogWarning(ex, ApplicationLogConstants.ChangeEmailLogs.CHANGE_EMAIL_SECURITY_NOTIFICATION_FAILED, currentUserPublicId);
        }
    }

}
