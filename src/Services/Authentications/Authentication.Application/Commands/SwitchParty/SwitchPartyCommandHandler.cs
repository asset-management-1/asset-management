namespace Authentication.Application.Commands.SwitchParty;

/// <summary>
/// Handles party-context switching for the authenticated user.
/// </summary>
public class SwitchPartyCommandHandler : ICommandHandler<SwitchPartyCommand, ResponseDto<SwitchPartyResponseDto>>
{
    private readonly IUserService _userService;
    private readonly ILogger<SwitchPartyCommandHandler> _logger;

    /// <summary>
    /// Creates the party-context switch handler with the user-context service.
    /// </summary>
    /// <param name="userService">The service that switches or creates the requested party context.</param>
    /// <param name="logger">The structured party-context workflow logger.</param>
    public SwitchPartyCommandHandler(
        IUserService userService,
        ILogger<SwitchPartyCommandHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Handles party-context switching for the authenticated user.
    /// </summary>
    /// <param name="request">The switch-party payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the resolved active context.</returns>
    public async ValueTask<ResponseDto<SwitchPartyResponseDto>> Handle(
        SwitchPartyCommand request,
        CancellationToken cancellationToken)
    {
        // UserService resolves the authenticated user/session and persists only the selected client context.
        var result = await _userService.SwitchPartyAsync(
            request.Adapt<SwitchPartyRequestDto>(),
            cancellationToken);

        // Record the context transition without duplicating profile or token data in Application logs.
        _logger.LogInformation(ApplicationLogConstants.UserLogs.PARTY_CONTEXT_SWITCHED);

        return new ResponseDto<SwitchPartyResponseDto>(result);
    }
}
