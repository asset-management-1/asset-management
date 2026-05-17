namespace Authentication.Application.Commands.SwitchParty;

/// <summary>
/// Handles party-context switching for the authenticated user.
/// </summary>
public class SwitchPartyCommandHandler : ICommandHandler<SwitchPartyCommand, ResponseDto<SwitchPartyResponseDto>>
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<SwitchPartyCommandHandler> _logger;

    /// <summary>
    /// Creates the party-context switch handler with current-user services and flow logging.
    /// </summary>
    /// <param name="userService">The service that switches or creates the requested party context.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    /// <param name="logger">The logger used for context-switch completion tracking.</param>
    public SwitchPartyCommandHandler(
        IUserService userService,
        IAuthService authService,
        ILogger<SwitchPartyCommandHandler> logger)
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Handles party-context switching for the authenticated user.
    /// </summary>
    /// <param name="request">The switch-party payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the resolved active context.</returns>
    public async ValueTask<ResponseDto<SwitchPartyResponseDto>> Handle(SwitchPartyCommand request, CancellationToken cancellationToken)
    {
        // Context switching uses the authenticated account and creates the target party context only when missing.
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);
        var result = await _userService.SwitchPartyAsync(
            request.Adapt<SwitchPartyRequestDto>(),
            cancellationToken);
        _logger.LogInformation(CONTEXT_SWITCHED, result.CurrentContext, currentUserPublicId);

        return new ResponseDto<SwitchPartyResponseDto>(result);
    }
}
