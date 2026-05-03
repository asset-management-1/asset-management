namespace Authentication.Application.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.UserName)
            .Required()
            .MaxLen(255);

        RuleFor(x => x.PartyType)
            .Required()
            .MaxLen(50);

        RuleFor(x => x.Password)
            .Required()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .Matches("[@$!%*?&]")
            .WithMessage(PASSWORD_COMPLEXITY_RULES);

        RuleFor(x => x.ConfirmPassword)
            .Required()
            .Equal(x => x.Password)
            .WithMessage(CONFIRM_PASSWORD_MUST_MATCH_PASSWORD);

        RuleFor(x => x.Email)
            .Required()
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.PhoneNumber)
            .Required()
            .MaxLen(50);

        RuleFor(x => x.FullName)
            .Required()
            .MaxLen(255);
    }
}
