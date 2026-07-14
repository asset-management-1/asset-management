namespace Authentication.Application.Commands.ExternalUnlink;

/// <summary>
/// Handles external-provider unlinking for the authenticated user.
/// </summary>
public class UnlinkExternalProviderCommandHandler : ICommandHandler<UnlinkExternalProviderCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IExternalAuthenticationService _externalAuthenticationService;
    private readonly IAuthService _authService;
    private readonly ILogger<UnlinkExternalProviderCommandHandler> _logger;

    /// <summary>
    /// Creates the external-provider unlink handler with current-user resolution and flow logging.
    /// </summary>
    /// <param name="externalAuthenticationService">The service that validates and persists external-provider unlinking.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    /// <param name="logger">The logger used for provider-unlink completion tracking.</param>
    public UnlinkExternalProviderCommandHandler(
        IExternalAuthenticationService externalAuthenticationService,
        IAuthService authService,
        ILogger<UnlinkExternalProviderCommandHandler> logger)
    {
        _externalAuthenticationService = externalAuthenticationService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Handles external-provider unlinking for the authenticated user.
    /// </summary>
    /// <param name="request">The external-provider unlink payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the provider-unlink success message.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        UnlinkExternalProviderCommand request,
        CancellationToken cancellationToken)
    {
        // Unlinking is scoped to the authenticated account and must not trust user identity from the request body.
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      ApplicationErrorConstants.ContextErrors.UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);

        // The service enforces supported-provider, ownership, and final sign-in method rules before persistence.
        var result = await _externalAuthenticationService.UnlinkAsync(
            request.Adapt<UnlinkExternalProviderRequestDto>(),
            currentUserPublicId,
            cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.ExternalProviderLogs.PROVIDER_UNLINKED, request.Provider, currentUserPublicId);

        return new ResponseDto<OperationStatusResponseDto>(result);
    }
}
