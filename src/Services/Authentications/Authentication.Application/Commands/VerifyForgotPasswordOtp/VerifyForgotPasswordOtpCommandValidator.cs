namespace Authentication.Application.Commands.VerifyForgotPasswordOtp;

public class VerifyForgotPasswordOtpCommandValidator : AbstractValidator<VerifyForgotPasswordOtpCommand>
{
    public VerifyForgotPasswordOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .Required()
            .EmailAddress();

        RuleFor(x => x.Otp)
            .Required()
            .Length(6);
    }
}
