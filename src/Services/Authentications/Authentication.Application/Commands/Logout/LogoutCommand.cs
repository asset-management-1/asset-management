namespace Authentication.Application.Commands.Logout;

/// <summary>
/// Represents a request to revoke the current client session.
/// </summary>
public class LogoutCommand : ICommand<ResponseDto<OperationStatusResponseDto>>
{
}
