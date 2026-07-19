namespace Authentication.Api.Controllers.v1;

/// <summary>
/// Exposes authentication endpoints for local sign-in, registration, session, external login, and password recovery flows.
/// </summary>
[ApiVersion(API_VERSION_1)]
public class AuthController : BaseApiController
{
    /// <summary>
    /// Authenticates by username/password and returns access + refresh tokens.
    /// </summary>
    /// <remarks>
    /// Requires the mobile client's stable device headers. A successful login creates or replaces only the session for
    /// that user and device, so sessions on other devices remain active.
    /// </remarks>
    /// <param name="request">The login request payload.</param>
    /// <returns>The standardized response containing the issued token pair.</returns>
    /// <response code="200">Credentials are valid and a device-scoped token pair is returned.</response>
    /// <response code="400">The request payload or required device metadata is invalid.</response>
    /// <response code="401">The username or password is invalid.</response>
    /// <response code="429">The login attempt limit has been reached.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [AllowAnonymous]
    [RequireClientDeviceInfo]
    [HttpPost]
    [Route(LOGIN)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.LoginRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Starts registration and sends an OTP to verify the email address.
    /// </summary>
    /// <remarks>
    /// Stores registration data temporarily and sends a verification OTP. The Haven account is created only after the
    /// OTP is successfully verified by the registration verification endpoint.
    /// </remarks>
    /// <param name="request">The registration request payload.</param>
    /// <returns>The standardized response describing the OTP-send result.</returns>
    /// <response code="200">The registration request is accepted and the OTP-send result is returned.</response>
    /// <response code="400">Registration fields are invalid or the account already exists.</response>
    /// <response code="429">The OTP request or verification-attempt limit has been reached.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    /// <response code="503">The OTP cache or notification dependency is unavailable.</response>
    [AllowAnonymous]
    [HttpPost]
    [Route(REGISTER)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.RegisterRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.RegisterResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Verifies the register email OTP and creates the account.
    /// </summary>
    /// <remarks>
    /// Atomically consumes the registration OTP so concurrent requests cannot create duplicate accounts. The client may
    /// continue to the login endpoint after registration succeeds.
    /// </remarks>
    /// <param name="request">The register email-verification payload.</param>
    /// <returns>The standardized response describing the registration-completion result.</returns>
    /// <response code="200">The OTP is valid and registration is completed.</response>
    /// <response code="400">The OTP, email, or cached registration state is invalid or expired.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [AllowAnonymous]
    [HttpPost]
    [Route($"{REGISTER}/{VERIFY_EMAIL}")]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.VerifyRegisterEmailRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.VerifyRegisterEmailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyRegisterEmail([FromBody] VerifyRegisterEmailCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Exchanges a valid refresh token for a new token pair.
    /// </summary>
    /// <remarks>
    /// Rotates the current device session under a database lock. An immediate retry with the previous token returns
    /// <c>409</c> without revoking the session; the mobile client must clear local tokens and sign in again.
    /// </remarks>
    /// <param name="request">The refresh-token request payload.</param>
    /// <returns>The standardized response containing the rotated token pair.</returns>
    /// <response code="200">The refresh token is current and a rotated token pair is returned.</response>
    /// <response code="400">The request payload or required device metadata is invalid.</response>
    /// <response code="401">The refresh token is invalid, expired, revoked, or belongs to another device.</response>
    /// <response code="409">The immediate previous refresh token was already processed.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [AllowAnonymous]
    [RequireClientDeviceInfo]
    [HttpPost]
    [Route(REFRESH_TOKEN)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.RefreshTokenRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.LoginResponse>(StatusCodes.Status200OK)]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.RefreshDuplicateResponse>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Revokes refresh token(s) of the current authenticated user.
    /// </summary>
    /// <remarks>
    /// Revokes only the session identified by the authenticated access token. Other device sessions for the same user
    /// remain active.
    /// </remarks>
    /// <returns>The standardized response describing the logout result.</returns>
    /// <response code="200">The current session is revoked.</response>
    /// <response code="400">The access token does not contain valid session context.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPost]
    [Route(LOGOUT)]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.LogoutResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout()
    {
        // Logout is driven by the authenticated bearer token and its trusted session_id claim.
        return Ok(await Mediator.Send(new LogoutCommand()));
    }

    /// <summary>
    /// Authenticates with Google or Facebook and returns either login tokens or registration prefill data.
    /// </summary>
    /// <remarks>
    /// Existing or safely auto-linked identities receive a token pair. A first-time identity receives
    /// <c>isNewRegistration=true</c> plus provider-derived prefill fields and must call complete-registration before any
    /// Haven account or login token is created.
    /// </remarks>
    /// <param name="request">The external-login request payload.</param>
    /// <returns>The standardized response containing exactly one external-login branch.</returns>
    /// <response code="200">The provider identity is valid; either login or registration-required data is returned.</response>
    /// <response code="400">The provider is unsupported or the provider identity has no usable email.</response>
    /// <response code="401">The external credential is invalid or expired.</response>
    /// <response code="409">The provider identity or verified email conflicts with another Haven account.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    /// <response code="503">The external provider or its metadata endpoint is unavailable.</response>
    [AllowAnonymous]
    [RequireClientDeviceInfo]
    [HttpPost]
    [Route(EXTERNAL_LOGIN)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.ExternalLoginRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.ExternalLoginResponse>(
        StatusCodes.Status200OK,
        Name = "existingAccount",
        Summary = "Existing account",
        Description = "The provider identity is linked or safely auto-linked, so Haven issues a login token pair.")]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.ExternalRegistrationRequiredResponse>(
        StatusCodes.Status200OK,
        Name = "registrationRequired",
        Summary = "Registration required",
        Description = "The provider identity is valid but has no Haven account; only provider prefill data is returned.")]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.ExternalUnauthorizedResponse>(StatusCodes.Status401Unauthorized)]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.ExternalConflictResponse>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ResponseDto<ExternalLoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Creates the Haven account after a first-time external identity completes registration fields.
    /// </summary>
    /// <remarks>
    /// Revalidates the Google or Facebook credential and derives identity fields from the provider again. A successful
    /// transaction creates the account graph and returns the first device-scoped Haven token pair.
    /// </remarks>
    /// <param name="request">The provider credential and user-confirmed registration fields.</param>
    /// <returns>The standardized response containing the first Haven login token pair.</returns>
    /// <response code="200">The external identity is registered and the first login token pair is returned.</response>
    /// <response code="400">Registration fields, party type, provider, or provider email are invalid.</response>
    /// <response code="401">The external credential is invalid or expired.</response>
    /// <response code="409">The provider identity or email was registered concurrently or belongs to another account.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    /// <response code="503">The external provider or its metadata endpoint is unavailable.</response>
    [AllowAnonymous]
    [RequireClientDeviceInfo]
    [HttpPost]
    [Route($"{EXTERNAL_LOGIN}/{COMPLETE_REGISTRATION}")]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.CompleteExternalRegistrationRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CompleteExternalRegistration([FromBody] CompleteExternalRegistrationCommand request)
    {
        // The application handler revalidates provider identity before creating the local account graph.
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Sends forgot-password OTP.
    /// </summary>
    /// <remarks>
    /// Always returns the same accepted response for existing and unknown emails to prevent account enumeration. An OTP
    /// is sent only when the account is eligible for password recovery.
    /// </remarks>
    /// <param name="request">The forgot-password request payload.</param>
    /// <returns>The standardized response describing the OTP-send result.</returns>
    /// <response code="200">The enumeration-safe forgot-password response is returned.</response>
    /// <response code="400">The email payload is malformed.</response>
    /// <response code="429">The OTP request limit has been reached.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [AllowAnonymous]
    [HttpPost]
    [Route(FORGOT_PASSWORD)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.ForgotPasswordRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.ForgotPasswordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Verifies forgot-password OTP.
    /// </summary>
    /// <remarks>
    /// Atomically consumes the OTP and returns a single-use opaque password-reset token valid for the documented TTL.
    /// The client must send that token, not the email or OTP, to the password-change endpoint.
    /// </remarks>
    /// <param name="request">The forgot-password OTP-verification payload.</param>
    /// <returns>The standardized response describing the OTP-verification result.</returns>
    /// <response code="200">The OTP is consumed and a short-lived password-reset token is returned.</response>
    /// <response code="400">The OTP or forgot-password state is invalid, expired, or already consumed.</response>
    /// <response code="429">The OTP verification-attempt limit has been reached.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [AllowAnonymous]
    [HttpPost]
    [Route($"{FORGOT_PASSWORD}/{VERIFY_OTP}")]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.VerifyForgotPasswordOtpRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.PasswordResetTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<PasswordResetTokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyForgotPasswordOtp([FromBody] VerifyForgotPasswordOtpCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Changes password after forgot-password OTP verification.
    /// </summary>
    /// <remarks>
    /// Consumes the opaque reset token before changing the password. Success revokes every existing user session and the
    /// client must navigate to login; a consumed token cannot be reused even when a later mutation fails.
    /// </remarks>
    /// <param name="request">The forgot-password change-password payload.</param>
    /// <returns>The standardized response describing the password-change result.</returns>
    /// <response code="200">The password is changed and all existing sessions are revoked.</response>
    /// <response code="400">The password policy or reset token is invalid, expired, or already consumed.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [AllowAnonymous]
    [HttpPost]
    [Route($"{FORGOT_PASSWORD}/{CHANGE_PASSWORD}")]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.ChangeForgotPasswordRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.ChangeForgotPasswordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeForgotPassword([FromBody] ChangeForgotPasswordCommand request)
    {
        return Ok(await Mediator.Send(request));
    }
}
