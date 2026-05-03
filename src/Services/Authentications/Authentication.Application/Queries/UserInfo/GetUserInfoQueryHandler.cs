namespace Authentication.Application.Queries.UserInfo;

/// <summary>
/// Handles retrieval of the current authenticated user information.
/// </summary>
public class GetUserInfoQueryHandler : IQueryHandler<GetUserInfoQuery, ResponseDto<UserInfoResponse>>
{
    private readonly IAuthenticationService _authenticationService;

    public GetUserInfoQueryHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Delegates the request to the authentication service.
    /// </summary>
    public Task<ResponseDto<UserInfoResponse>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        return _authenticationService.GetUserInfoAsync(cancellationToken);
    }
}
