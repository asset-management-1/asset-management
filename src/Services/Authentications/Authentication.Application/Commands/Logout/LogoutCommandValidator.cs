namespace Authentication.Application.Commands.Logout;

/// <summary>
/// Validates logout requests for current-session revocation.
/// </summary>
public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    /// <summary>
    /// Creates validation rules for logout mode.
    /// </summary>
    public LogoutCommandValidator()
    {
        // Logout is scoped by the trusted session_id claim; no request token field is needed.
    }
}
