namespace Authentication.Application.Queries.UserInfo;

/// <summary>
/// Handles retrieval of the current authenticated user information.
/// </summary>
public class GetUserInfoQueryHandler : IQueryHandler<GetUserInfoQuery, ResponseDto<UserInfoResponseDto>>
{
    private readonly IUserService _userService;

    /// <summary>
    /// Creates the current-user info query handler with profile read services.
    /// </summary>
    /// <param name="userService">The service that loads the current-user profile read model.</param>
    public GetUserInfoQueryHandler(IUserService userService)
    {
        _userService = userService;
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

        return new ResponseDto<UserInfoResponseDto>(result);
    }
}
