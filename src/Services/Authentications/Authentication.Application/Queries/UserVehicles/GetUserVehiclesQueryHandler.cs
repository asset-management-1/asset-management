namespace Authentication.Application.Queries.UserVehicles;

/// <summary>
/// Handles retrieval of current tenant profile vehicles.
/// </summary>
public class GetUserVehiclesQueryHandler : IQueryHandler<GetUserVehiclesQuery, ResponseDto<IReadOnlyList<UserVehicleResponseDto>>>
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<GetUserVehiclesQueryHandler> _logger;

    /// <summary>
    /// Creates the tenant vehicle query handler.
    /// </summary>
    /// <param name="userService">The service that loads active tenant profile vehicles.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    /// <param name="logger">The logger used for query completion tracking.</param>
    public GetUserVehiclesQueryHandler(
        IUserService userService,
        IAuthService authService,
        ILogger<GetUserVehiclesQueryHandler> logger)
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Loads active vehicles under the current tenant profile.
    /// </summary>
    /// <param name="request">The tenant vehicle query.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response containing active tenant vehicles.</returns>
    public async ValueTask<ResponseDto<IReadOnlyList<UserVehicleResponseDto>>> Handle(
        GetUserVehiclesQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);

        // Listing is tenant-context scoped because vehicles are profile data, not account-wide identity data.
        var result = await _userService.GetVehiclesAsync(cancellationToken);
        _logger.LogInformation(USER_VEHICLES_LOADED, currentUserPublicId);

        return new ResponseDto<IReadOnlyList<UserVehicleResponseDto>>(result);
    }
}
