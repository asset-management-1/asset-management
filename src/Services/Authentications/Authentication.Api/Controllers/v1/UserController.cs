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
    /// <remarks>
    /// Verifies the current password under a user/session lock, revokes every other device session, and rotates the
    /// current session. The mobile client must replace both locally stored tokens with the returned pair.
    /// </remarks>
    /// <param name="request">The authenticated change-password payload.</param>
    /// <returns>The standardized response describing the password-change result.</returns>
    /// <response code="200">The password is changed and the current device receives a rotated token pair.</response>
    /// <response code="400">The current password, new password, or required device metadata is invalid.</response>
    /// <response code="401">The caller or current session is not authenticated.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPost]
    [RequireClientDeviceInfo]
    [Route(CHANGE_PASSWORD)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.ChangePasswordRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.ChangePasswordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<ChangePasswordResponseDto>), StatusCodes.Status200OK)]
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
    /// <remarks>
    /// Accepts multipart form data so text fields and an optional avatar can be updated together. Existing values are
    /// retained for optional fields that are not supplied.
    /// </remarks>
    /// <param name="request">The profile fields to update.</param>
    /// <returns>The standardized response describing the profile update result.</returns>
    /// <response code="200">The submitted profile fields are updated.</response>
    /// <response code="400">A profile field or uploaded avatar is invalid.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="413">The uploaded file exceeds the configured request limit.</response>
    /// <response code="415">The request content type or uploaded file type is unsupported.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    /// <response code="503">The profile storage dependency is unavailable.</response>
    [HttpPut]
    [Route(USER_INFO)]
    [Consumes(MULTIPART_FORM_DATA)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.UpdateUserInfoRequest>]
    [SwaggerValueExample<AuthenticationSwaggerExamples.UpdateUserInfoFieldValues>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.UpdateUserInfoResponse>(StatusCodes.Status200OK)]
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
    /// <remarks>
    /// Sends an OTP to the proposed new email while keeping the current account email unchanged. The change is applied
    /// only after the OTP is verified by the change-email verification endpoint.
    /// </remarks>
    /// <param name="request">The new-email payload.</param>
    /// <returns>The standardized response describing the OTP send result.</returns>
    /// <response code="200">The new email is eligible and the OTP-send result is returned.</response>
    /// <response code="400">The new email is invalid, unchanged, or already belongs to another account.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="429">The OTP request limit has been reached.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    /// <response code="503">The OTP cache or notification dependency is unavailable.</response>
    [HttpPost]
    [Route(CHANGE_EMAIL)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.ChangeEmailRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.ChangeEmailResponse>(StatusCodes.Status200OK)]
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
    /// <remarks>
    /// Atomically consumes the OTP before updating the account email, preventing concurrent verification requests from
    /// applying the same email-change authority more than once.
    /// </remarks>
    /// <param name="request">The change-email OTP verification payload.</param>
    /// <returns>The standardized response describing the email-change result.</returns>
    /// <response code="200">The OTP is valid and the account email is changed.</response>
    /// <response code="400">The OTP or pending email-change state is invalid, expired, or already consumed.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPost]
    [Route(CHANGE_EMAIL_VERIFY_OTP)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.VerifyChangeEmailOtpRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.VerifyChangeEmailOtpResponse>(StatusCodes.Status200OK)]
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
    /// <remarks>
    /// Accepts multipart identity fields plus front/back document scans. A successful response means the submission is
    /// queued for review; it does not mean the user has already passed KYC.
    /// </remarks>
    /// <param name="request">The KYC form fields and private document scans.</param>
    /// <returns>The standardized response containing KYC submission status.</returns>
    /// <response code="200">The KYC submission is accepted for review.</response>
    /// <response code="400">Identity fields or required document scans are invalid.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="413">An uploaded document exceeds the configured request limit.</response>
    /// <response code="415">The request content type or uploaded document type is unsupported.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    /// <response code="503">The private document storage dependency is unavailable.</response>
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
    /// <remarks>
    /// Revalidates the Google or Facebook credential and links its provider identity to the current account. The
    /// provider identity cannot already belong to another Haven user.
    /// </remarks>
    /// <param name="request">The external-provider link payload.</param>
    /// <returns>The standardized response describing the link result.</returns>
    /// <response code="200">The provider identity is linked to the current account.</response>
    /// <response code="400">The provider, external credential payload, or provider email is invalid.</response>
    /// <response code="401">The caller or external credential is unauthorised.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    /// <response code="503">The external provider or its metadata endpoint is unavailable.</response>
    [HttpPost]
    [Route(EXTERNAL_LINK)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.LinkExternalProviderRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.LinkExternalProviderResponse>(StatusCodes.Status200OK)]
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
    /// <remarks>
    /// Removes the selected provider mapping only when another sign-in method remains. An account cannot unlink its last
    /// usable authentication method.
    /// </remarks>
    /// <param name="request">The external-provider unlink payload.</param>
    /// <returns>The standardized response describing the unlink result.</returns>
    /// <response code="200">The selected provider is unlinked.</response>
    /// <response code="400">The provider is unsupported, not linked, or is the account's last sign-in method.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPost]
    [Route(EXTERNAL_UNLINK)]
    [SwaggerRequestExample<AuthenticationSwaggerExamples.UnlinkExternalProviderRequest>]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.UnlinkExternalProviderResponse>(StatusCodes.Status200OK)]
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
    /// <remarks>
    /// Returns profile data, the active party context, available contexts, linked external providers, KYC summary, and
    /// tenant vehicles visible to the current authenticated user.
    /// </remarks>
    /// <returns>The standardized response containing the current-user profile.</returns>
    /// <response code="200">The current-user profile and context summary are returned.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpGet]
    [Route(USER_INFO)]
    [SwaggerResponseExample<AuthenticationSwaggerExamples.UserInfoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<UserInfoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserInfo()
    {
        return Ok(await Mediator.Send(new GetUserInfoQuery()));
    }

    /// <summary>
    /// Switches the current authenticated user to another party context.
    /// </summary>
    /// <remarks>
    /// Changes the active application context only when the requested party is linked to the current user. This does not
    /// create a new party or modify the user's available roles.
    /// </remarks>
    /// <param name="request">The target party-context payload.</param>
    /// <returns>The standardized response containing the resolved active context.</returns>
    /// <response code="200">The requested linked party becomes the active context.</response>
    /// <response code="400">The target party is invalid or is not linked to the current user.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="500">An unexpected server error occurred.</response>
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
