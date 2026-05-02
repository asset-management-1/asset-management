namespace Authentication.Application.Queries.Logins;

public class GetLoginQueryHandler : IQueryHandler<GetLoginQuery, ResponseDto<LoginResponse>>
{
    private readonly IAuthenticationService _authenticationService;

    public GetLoginQueryHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Handles username/password login request and returns access/refresh token pair.
    /// </summary>
    public Task<ResponseDto<LoginResponse>> Handle(GetLoginQuery request, CancellationToken cancellationToken)
    {
        return _authenticationService.LoginAsync(request.UserName, request.Password, cancellationToken);
    }
}
