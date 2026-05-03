namespace Authentication.Application.Commands.VerifyRegisterEmail;

public class VerifyRegisterEmailCommandValidator : AbstractValidator<VerifyRegisterEmailCommand>
{
    public VerifyRegisterEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .Required()
            .EmailAddress();

        RuleFor(x => x.Otp)
            .Required()
            .Length(6);
    }
}
