namespace Authentication.Application.Commands.ExternalLink;

/// <summary>
/// Handles external-provider link requests for the current authenticated user.
/// </summary>
public class LinkExternalProviderCommandHandler : ICommandHandler<LinkExternalProviderCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IExternalAuthenticationService _externalAuthenticationService;
    private readonly IAuthService _authService;

    /// <summary>
    /// Creates the external-provider link handler with current-user resolution.
    /// </summary>
    /// <param name="externalAuthenticationService">The service that validates and persists external-provider links.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    public LinkExternalProviderCommandHandler(
        IExternalAuthenticationService externalAuthenticationService,
        IAuthService authService)
    {
        _externalAuthenticationService = externalAuthenticationService;
        _authService = authService;
    }

    /// <summary>
    /// Handles external-provider linking for the authenticated user.
    /// </summary>
    /// <param name="request">The external-provider link payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the provider-link success message.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        LinkExternalProviderCommand request,
        CancellationToken cancellationToken)
    {
        // Provider links are owned by the authenticated account, so identity comes from the normalised token only.
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);

        // ExternalAuthenticationService validates the provider token and persists the link under one transaction.
        var result = await _externalAuthenticationService.LinkAsync(
            request.Adapt<LinkExternalProviderRequestDto>(),
            currentUserPublicId,
            cancellationToken);
        return new ResponseDto<OperationStatusResponseDto>(result);
    }
}
