namespace Authentication.Application.Commands.VerifyRegisterEmail;

/// <summary>
/// Validates register email verification requests.
/// </summary>
public class VerifyRegisterEmailCommandValidator : AbstractValidator<VerifyRegisterEmailCommand>
{
    /// <summary>
    /// Creates validation rules for the pending email and OTP fields.
    /// </summary>
    public VerifyRegisterEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .Required()
            .EmailAddress();

        RuleFor(x => x.Otp)
            .Required()
            .Length(OTP_LENGTH);
    }
}
