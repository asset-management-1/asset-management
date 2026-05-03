namespace Authentication.Api.Controllers.v1;

[ApiVersion(API_VERSION_1)]
public class AuthController : BaseApiController
{
    /// <summary>
    /// Authenticates by username/password and returns access + refresh tokens.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [Route(LOGIN)]
    public async Task<IActionResult> Login([FromBody] GetLoginQuery request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Starts registration and sends an OTP to verify the email address.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [Route(REGISTER)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Verifies the register email OTP and creates the account.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [Route($"{REGISTER}/{VERIFY_EMAIL}")]
    public async Task<IActionResult> VerifyRegisterEmail([FromBody] VerifyRegisterEmailCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Exchanges a valid refresh token for a new token pair.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [Route(REFRESH_TOKEN)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Revokes refresh token(s) of the current authenticated user.
    /// </summary>
    [HttpPost]
    [Route(LOGOUT)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Authenticates via third-party provider and returns token pair.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [Route(EXTERNAL_LOGIN)]
    public async Task<IActionResult> ExternalLogin([FromBody] ThirdPartyLoginCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Sends forgot-password OTP.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [Route(FORGOT_PASSWORD)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Verifies forgot-password OTP.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [Route($"{FORGOT_PASSWORD}/{VERIFY_OTP}")]
    public async Task<IActionResult> VerifyForgotPasswordOtp([FromBody] VerifyForgotPasswordOtpCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Changes password after forgot-password OTP verification.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [Route($"{FORGOT_PASSWORD}/{CHANGE_PASSWORD}")]
    public async Task<IActionResult> ChangeForgotPassword([FromBody] ChangeForgotPasswordCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Changes password for the current authenticated user.
    /// </summary>
    [HttpPost]
    [Route(CHANGE_PASSWORD)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Links an external provider to the current authenticated user.
    /// </summary>
    [HttpPost]
    [Route(EXTERNAL_LINK)]
    public async Task<IActionResult> LinkExternalProvider([FromBody] LinkExternalProviderCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Returns current user info.
    /// </summary>
    [HttpGet]
    [Route(USER_INFO)]
    public async Task<IActionResult> GetUserInfo()
    {
        return Ok(await Mediator.Send(new GetUserInfoQuery()));
    }
}
