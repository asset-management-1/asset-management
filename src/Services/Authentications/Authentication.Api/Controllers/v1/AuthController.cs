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
    /// <param name="request">The login request payload.</param>
    /// <returns>The standardized response containing the issued token pair.</returns>
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
    /// <param name="request">The registration request payload.</param>
    /// <returns>The standardized response describing the OTP-send result.</returns>
    [AllowAnonymous]
    [HttpPost]
    [Route(REGISTER)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.RegisterRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
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
    /// <param name="request">The register email-verification payload.</param>
    /// <returns>The standardized response describing the registration-completion result.</returns>
    [AllowAnonymous]
    [HttpPost]
    [Route($"{REGISTER}/{VERIFY_EMAIL}")]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.VerifyRegisterEmailRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
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
    /// <param name="request">The refresh-token request payload.</param>
    /// <returns>The standardized response containing the rotated token pair.</returns>
    [AllowAnonymous]
    [RequireClientDeviceInfo]
    [HttpPost]
    [Route(REFRESH_TOKEN)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.RefreshTokenRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Revokes refresh token(s) of the current authenticated user.
    /// </summary>
    /// <returns>The standardized response describing the logout result.</returns>
    [HttpPost]
    [Route(LOGOUT)]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
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
    /// Authenticates via external provider and returns token pair.
    /// </summary>
    /// <param name="request">The external-login request payload.</param>
    /// <returns>The standardized response containing the issued token pair.</returns>
    [AllowAnonymous]
    [RequireClientDeviceInfo]
    [HttpPost]
    [Route(EXTERNAL_LOGIN)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.ExternalLoginRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Sends forgot-password OTP.
    /// </summary>
    /// <param name="request">The forgot-password request payload.</param>
    /// <returns>The standardized response describing the OTP-send result.</returns>
    [AllowAnonymous]
    [HttpPost]
    [Route(FORGOT_PASSWORD)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.ForgotPasswordRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
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
    /// <param name="request">The forgot-password OTP-verification payload.</param>
    /// <returns>The standardized response describing the OTP-verification result.</returns>
    [AllowAnonymous]
    [HttpPost]
    [Route($"{FORGOT_PASSWORD}/{VERIFY_OTP}")]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.VerifyForgotPasswordOtpRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
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
    /// <param name="request">The forgot-password change-password payload.</param>
    /// <returns>The standardized response describing the password-change result.</returns>
    [AllowAnonymous]
    [HttpPost]
    [Route($"{FORGOT_PASSWORD}/{CHANGE_PASSWORD}")]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.ChangeForgotPasswordRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeForgotPassword([FromBody] ChangeForgotPasswordCommand request)
    {
        return Ok(await Mediator.Send(request));
    }
}
