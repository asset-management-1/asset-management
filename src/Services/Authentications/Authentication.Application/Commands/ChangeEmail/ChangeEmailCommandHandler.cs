namespace Authentication.Application.Commands.ChangeEmail;

/// <summary>
/// Handles change-email OTP send and resend requests.
/// </summary>
public class ChangeEmailCommandHandler : ICommandHandler<ChangeEmailCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authenticationService;
    private readonly ICachingService _cachingService;
    private readonly IAuthService _authService;
    private readonly ILogger<ChangeEmailCommandHandler> _logger;

    /// <summary>
    /// Creates the change-email OTP handler with current-user validation, cache state, email delivery, and flow logging dependencies.
    /// </summary>
    /// <param name="userService">The service that validates current-user email changes.</param>
    /// <param name="authenticationService">The service that sends auth-related emails.</param>
    /// <param name="cachingService">The cache service used for change-email session and cooldown state.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    /// <param name="logger">The logger used for non-blocking old-email notification failures.</param>
    public ChangeEmailCommandHandler(
        IUserService userService,
        IAuthenticationService authenticationService,
        ICachingService cachingService,
        IAuthService authService,
        ILogger<ChangeEmailCommandHandler> logger)
    {
        _userService = userService;
        _authenticationService = authenticationService;
        _cachingService = cachingService;
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
                                      UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);

        // Reject resend while the current user's change-email cooldown marker still exists.
        var hasCooldown = !string.IsNullOrWhiteSpace(await _cachingService.GetAsync<string>(
            AuthenticationFlowHelper.BuildChangeEmailCooldownKey(currentUserPublicId),
            cancellationToken));
        if (hasCooldown)
        {
            throw new ApiException(
                OTP_COOLDOWN_MESSAGE,
                AUTH_OTP_COOLDOWN,
                StatusCodes.Status429TooManyRequests);
        }

        _logger.LogInformation(CHANGE_EMAIL_FLOW_STEP1_CURRENT_USER_RESOLVED);

        // UserService validates ownership rules and normalizes the target email for the session cache below.
        var changeEmail = await _userService.PrepareChangeEmailAsync(
            request.Adapt<ChangeEmailRequestDto>(),
            cancellationToken);
        _logger.LogInformation(CHANGE_EMAIL_FLOW_STEP2_TARGET_PREPARED);

        var otpCode = AuthenticationFlowHelper.GenerateOtp();
        var otpCacheResponse = new OtpCacheRequestDto
        {
            Purpose = CHANGE_EMAIL_PURPOSE,
            NormalizedEmail = changeEmail.NewEmail,
            OtpCode = otpCode
        }.Adapt<OtpCacheResponseDto>();
        otpCacheResponse.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(OTP_TTL_MINUTES);

        var otpTtl = TimeSpan.FromMinutes(OTP_TTL_MINUTES);
        var changeEmailSessionKey = AuthenticationFlowHelper.BuildChangeEmailSessionKey(currentUserPublicId);
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
        _logger.LogInformation(CHANGE_EMAIL_FLOW_STEP3_SESSION_CACHED);

        // The new email must receive the OTP before cooldown/security notification is recorded.
        var sent = await _authenticationService.SendOtpEmailAsync(
            new OtpEmailRequestDto
            {
                Email = changeEmail.NewEmail,
                Subject = EMAIL_SUBJECT_CHANGE_EMAIL,
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
            _logger.LogWarning(CHANGE_EMAIL_FLOW_ROLLBACK_OTP_SEND_FAILED);
            throw new ApiException(
                OTP_SEND_FAILED_MESSAGE,
                AUTH_FORBIDDEN_OPERATION,
                StatusCodes.Status503ServiceUnavailable);
        }

        _logger.LogInformation(CHANGE_EMAIL_FLOW_STEP4_OTP_SENT);

        // Old-email notification is best-effort and must not block ownership verification of the new email.
        await TrySendSecurityNotificationAsync(changeEmail, currentUserPublicId, cancellationToken);
        _logger.LogInformation(CHANGE_EMAIL_FLOW_STEP5_SECURITY_NOTIFICATION_ATTEMPTED);

        await _cachingService.SetAbsoluteAsync(
            AuthenticationFlowHelper.BuildChangeEmailCooldownKey(currentUserPublicId),
            OTP_COOLDOWN_VALUE,
            TimeSpan.FromSeconds(OTP_COOLDOWN_SECONDS),
            cancellationToken);
        _logger.LogInformation(CHANGE_EMAIL_FLOW_STEP6_COOLDOWN_SET);
        _logger.LogInformation(CHANGE_EMAIL_FLOW_COMPLETED, currentUserPublicId);

        return new ResponseDto<OperationStatusResponseDto>(
            OperationStatusResponseHelper.Success(CHANGE_EMAIL_OTP_SENT_MESSAGE));
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
            var sent = await _authenticationService.SendChangeEmailSecurityNotificationAsync(
                changeEmail.Adapt<ChangeEmailSecurityNotificationRequestDto>(),
                cancellationToken);
            if (!sent.IsSuccess)
            {
                _logger.LogWarning(CHANGE_EMAIL_SECURITY_NOTIFICATION_FAILED, currentUserPublicId);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, CHANGE_EMAIL_SECURITY_NOTIFICATION_FAILED, currentUserPublicId);
        }
    }

}
