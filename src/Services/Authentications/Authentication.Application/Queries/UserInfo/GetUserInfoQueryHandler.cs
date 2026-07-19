namespace Authentication.Application.Queries.UserInfo;

/// <summary>
/// Handles retrieval of the current authenticated user information.
/// </summary>
public class GetUserInfoQueryHandler : IQueryHandler<GetUserInfoQuery, ResponseDto<UserInfoResponseDto>>
{
    private readonly IUserService _userService;
    private readonly ILogger<GetUserInfoQueryHandler> _logger;

    /// <summary>
    /// Creates the current-user info query handler with profile read services.
    /// </summary>
    /// <param name="userService">The service that loads the current-user profile read model.</param>
    /// <param name="logger">The structured current-user query logger.</param>
    public GetUserInfoQueryHandler(
        IUserService userService,
        ILogger<GetUserInfoQueryHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Loads the current authenticated user's profile.
    /// </summary>
    /// <param name="request">The current-user info query.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the current-user profile.</returns>
    public async ValueTask<ResponseDto<UserInfoResponseDto>> Handle(
        GetUserInfoQuery request,
        CancellationToken cancellationToken)
    {
        // User info is loaded per authenticated session because two devices may select different Party contexts.
        var result = await _userService.GetUserInfoAsync(cancellationToken);

        // Record successful application orchestration without logging profile fields or contact data.
        _logger.LogInformation(ApplicationLogConstants.UserLogs.USER_INFO_LOADED);

        return new ResponseDto<UserInfoResponseDto>(result);
    }
}
