namespace Authentication.Application.Commands.Logout;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => x.LogoutAllSessions || !string.IsNullOrWhiteSpace(x.RefreshToken))
            .WithMessage("RefreshToken is required when LogoutAllSessions is false.");
    }
}
