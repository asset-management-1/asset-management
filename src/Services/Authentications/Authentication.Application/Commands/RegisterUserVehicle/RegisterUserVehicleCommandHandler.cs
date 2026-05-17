namespace Authentication.Application.Commands.RegisterUserVehicle;

/// <summary>
/// Handles tenant profile vehicle registration.
/// </summary>
public class RegisterUserVehicleCommandHandler : ICommandHandler<RegisterUserVehicleCommand, ResponseDto<UserVehicleResponseDto>>
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<RegisterUserVehicleCommandHandler> _logger;

    /// <summary>
    /// Creates the tenant vehicle registration handler.
    /// </summary>
    /// <param name="userService">The service that persists tenant profile vehicles.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    /// <param name="logger">The logger used for command completion tracking.</param>
    public RegisterUserVehicleCommandHandler(
        IUserService userService,
        IAuthService authService,
        ILogger<RegisterUserVehicleCommandHandler> logger)
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a vehicle under the current tenant profile.
    /// </summary>
    /// <param name="request">The vehicle registration command.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response containing the registered vehicle.</returns>
    public async ValueTask<ResponseDto<UserVehicleResponseDto>> Handle(
        RegisterUserVehicleCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);

        // Vehicle registration is a single profile write; tenant ownership and duplicate checks live in UserService.
        var result = await _userService.RegisterVehicleAsync(
            request.Adapt<RegisterUserVehicleRequestDto>(),
            cancellationToken);
        _logger.LogInformation(USER_VEHICLE_REGISTERED, currentUserPublicId);

        return new ResponseDto<UserVehicleResponseDto>(result);
    }
}
