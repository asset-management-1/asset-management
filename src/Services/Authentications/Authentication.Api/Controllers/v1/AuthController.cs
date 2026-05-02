using Authentication.Application.Commands.ForgotPassword;
using Authentication.Application.Commands.Logout;
using Authentication.Application.Commands.RefreshToken;
using Authentication.Application.Commands.Register;
using Authentication.Application.Commands.ThirdPartyLogin;
using Authentication.Application.Queries.Logins;

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
    /// Registers a new user and returns access + refresh tokens.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [Route(REGISTER)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand request)
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
    [Route(THIRD_PARTY_LOGIN)]
    public async Task<IActionResult> ThirdPartyLogin([FromBody] ThirdPartyLoginCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Handles forgot password by resetting account password.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [Route(FORGOT_PASSWORD)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand request)
    {
        return Ok(await Mediator.Send(request));
    }
}
