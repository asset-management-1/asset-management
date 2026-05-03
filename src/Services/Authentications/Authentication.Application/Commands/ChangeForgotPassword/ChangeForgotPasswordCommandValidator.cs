namespace Authentication.Application.Commands.ChangeForgotPassword;

public class ChangeForgotPasswordCommandValidator : AbstractValidator<ChangeForgotPasswordCommand>
{
    public ChangeForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .Required()
            .EmailAddress();

        RuleFor(x => x.NewPassword)
            .Required()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .Matches("[@$!%*?&]")
            .WithMessage(PASSWORD_COMPLEXITY_RULES);

        RuleFor(x => x.ConfirmPassword)
            .Required()
            .Equal(x => x.NewPassword)
            .WithMessage(CONFIRM_PASSWORD_MUST_MATCH_NEW_PASSWORD);
    }
}
