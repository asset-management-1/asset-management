namespace Authentication.Api.Controllers.v1;

/// <summary>
/// Exposes current-user account, security, external-provider, and party-context endpoints.
/// </summary>
[ApiVersion(API_VERSION_1)]
public class UserController : BaseApiController
{
    /// <summary>
    /// Changes password for the current authenticated user.
    /// </summary>
    /// <param name="request">The authenticated change-password payload.</param>
    /// <returns>The standardized response describing the password-change result.</returns>
    [HttpPost]
    [Route(CHANGE_PASSWORD)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.ChangePasswordRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Updates profile fields for the current authenticated user.
    /// </summary>
    /// <param name="request">The profile fields to update.</param>
    /// <returns>The standardized response describing the profile update result.</returns>
    [HttpPut]
    [Route(USER_INFO)]
    [Consumes(MULTIPART_FORM_DATA)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.UpdateUserInfoRequest>]
    [SwaggerValueExample<AuthenticationSwaggerExamples.UpdateUserInfoFieldValues>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUserInfo([FromForm] UpdateUserInfoCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Starts or resends a change-email OTP for the current authenticated user.
    /// </summary>
    /// <param name="request">The new-email payload.</param>
    /// <returns>The standardized response describing the OTP send result.</returns>
    [HttpPost]
    [Route(CHANGE_EMAIL)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.ChangeEmailRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Verifies the change-email OTP and applies the new email.
    /// </summary>
    /// <param name="request">The change-email OTP verification payload.</param>
    /// <returns>The standardized response describing the email-change result.</returns>
    [HttpPost]
    [Route(CHANGE_EMAIL_VERIFY_OTP)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.VerifyChangeEmailOtpRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyChangeEmailOtp([FromBody] VerifyChangeEmailOtpCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Submits identity-document KYC data and private scan files for manual admin review.
    /// </summary>
    /// <param name="request">The KYC form fields and private document scans.</param>
    /// <returns>The standardized response containing KYC submission status.</returns>
    [HttpPost]
    [Route(KYC)]
    [Consumes(MULTIPART_FORM_DATA)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.SubmitKycRequest>]
    [SwaggerValueExample<AuthenticationSwaggerExamples.SubmitKycFieldValues>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.KycSubmissionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<KycSubmissionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SubmitKyc([FromForm] SubmitKycCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Links an external provider to the current authenticated user.
    /// </summary>
    /// <param name="request">The external-provider link payload.</param>
    /// <returns>The standardized response describing the link result.</returns>
    [HttpPost]
    [Route(EXTERNAL_LINK)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.LinkExternalProviderRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LinkExternalProvider([FromBody] LinkExternalProviderCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Unlinks an external provider from the current authenticated user.
    /// </summary>
    /// <param name="request">The external-provider unlink payload.</param>
    /// <returns>The standardized response describing the unlink result.</returns>
    [HttpPost]
    [Route(EXTERNAL_UNLINK)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.UnlinkExternalProviderRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<OperationStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UnlinkExternalProvider([FromBody] UnlinkExternalProviderCommand request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Returns current user info.
    /// </summary>
    /// <param name="request">Optional cache-control query values for the user-info read.</param>
    /// <returns>The standardized response containing the current-user profile.</returns>
    [HttpGet]
    [Route(USER_INFO)]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.UserInfoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<UserInfoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserInfo([FromQuery] GetUserInfoQuery request)
    {
        return Ok(await Mediator.Send(request));
    }

    /// <summary>
    /// Switches the current authenticated user to another party context.
    /// </summary>
    /// <param name="request">The target party-context payload.</param>
    /// <returns>The standardized response containing the resolved active context.</returns>
    [HttpPost]
    [Route(SWITCH_PARTY)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.SwitchPartyRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.SwitchPartyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<SwitchPartyResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SwitchParty([FromBody] SwitchPartyCommand request)
    {
        return Ok(await Mediator.Send(request));
    }
}
