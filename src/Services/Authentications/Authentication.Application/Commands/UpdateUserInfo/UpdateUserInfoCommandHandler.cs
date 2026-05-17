namespace Authentication.Application.Commands.UpdateUserInfo;

/// <summary>
/// Handles current-user profile update requests.
/// </summary>
public class UpdateUserInfoCommandHandler : ICommandHandler<UpdateUserInfoCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<UpdateUserInfoCommandHandler> _logger;

    /// <summary>
    /// Creates the current-user profile update handler with profile services and flow logging.
    /// </summary>
    /// <param name="userService">The service that updates profile data and linked party contacts.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    /// <param name="logger">The logger used for profile-update completion tracking.</param>
    public UpdateUserInfoCommandHandler(
        IUserService userService,
        IAuthService authService,
        ILogger<UpdateUserInfoCommandHandler> logger)
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Handles profile updates for the current authenticated user.
    /// </summary>
    /// <param name="request">The profile update payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response that wraps the update result.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        UpdateUserInfoCommand request,
        CancellationToken cancellationToken)
    {
        // Profile updates are account-scoped and also sync linked party contact snapshots in UserService.
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);
        var result = await _userService.UpdateUserInfoAsync(request.Adapt<UpdateUserInfoRequestDto>(), cancellationToken);
        _logger.LogInformation(USER_INFO_UPDATED, currentUserPublicId);

        return new ResponseDto<OperationStatusResponseDto>(result);
    }
}
