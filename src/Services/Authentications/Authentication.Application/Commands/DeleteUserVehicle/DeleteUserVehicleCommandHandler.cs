namespace Authentication.Application.Commands.DeleteUserVehicle;

/// <summary>
/// Handles tenant profile vehicle removal.
/// </summary>
public class DeleteUserVehicleCommandHandler : ICommandHandler<DeleteUserVehicleCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<DeleteUserVehicleCommandHandler> _logger;

    /// <summary>
    /// Creates the tenant vehicle delete handler.
    /// </summary>
    /// <param name="userService">The service that enforces tenant ownership and deletes vehicles.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    /// <param name="logger">The logger used for command completion tracking.</param>
    public DeleteUserVehicleCommandHandler(
        IUserService userService,
        IAuthService authService,
        ILogger<DeleteUserVehicleCommandHandler> logger)
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Soft deletes a vehicle from the current tenant profile.
    /// </summary>
    /// <param name="request">The vehicle delete command.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response describing the delete result.</returns>
    public async ValueTask<ResponseDto<OperationStatusResponseDto>> Handle(
        DeleteUserVehicleCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);

        // Delete is scoped by current tenant party, so a route id cannot remove another tenant's vehicle.
        var result = await _userService.DeleteVehicleAsync(request.VehiclePublicId, cancellationToken);
        _logger.LogInformation(USER_VEHICLE_DELETED, currentUserPublicId);

        return new ResponseDto<OperationStatusResponseDto>(result);
    }
}
