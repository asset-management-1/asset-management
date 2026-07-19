namespace Authentication.Application.Commands.UpdateUserInfo;

/// <summary>
/// Handles current-user profile update requests.
/// </summary>
public class UpdateUserInfoCommandHandler : ICommandHandler<UpdateUserInfoCommand, ResponseDto<OperationStatusResponseDto>>
{
    private readonly IUserService _userService;

    /// <summary>
    /// Creates the current-user profile update handler with the profile service.
    /// </summary>
    /// <param name="userService">The service that updates profile data and linked party contacts.</param>
    public UpdateUserInfoCommandHandler(IUserService userService)
    {
        _userService = userService;
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
        // UserService resolves the authenticated account and synchronizes linked party contact snapshots.
        var result = await _userService.UpdateUserInfoAsync(request.Adapt<UpdateUserInfoRequestDto>(), cancellationToken);

        return new ResponseDto<OperationStatusResponseDto>(result);
    }
}
