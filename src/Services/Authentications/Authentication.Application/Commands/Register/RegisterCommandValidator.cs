namespace Authentication.Application.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.UserName)
            .Required()
            .MaxLen(255);

        RuleFor(x => x.Password)
            .Required()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .Matches("[@$!%*?&]")
            .WithMessage(PASSWORD_COMPLEXITY_RULES);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.FullName)
            .Required()
            .MaxLen(255);
    }
}
