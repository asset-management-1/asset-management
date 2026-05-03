namespace Authentication.Application.Commands.Logout;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => x.LogoutAllSessions || !string.IsNullOrWhiteSpace(x.RefreshToken))
            .WithMessage(REFRESH_TOKEN_REQUIRED_WHEN_LOGOUT_SINGLE_SESSION);
    }
}
