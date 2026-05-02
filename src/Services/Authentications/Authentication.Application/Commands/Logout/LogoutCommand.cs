namespace Authentication.Application.Commands.Logout;

public class LogoutCommand : ICommand<ResponseDto<string>>
{
    /// <summary>
    /// Specific refresh token to revoke. If empty and LogoutAllSessions is true, all sessions are revoked.
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// Indicates whether to revoke all active sessions for current user.
    /// </summary>
    public bool LogoutAllSessions { get; set; }
}
