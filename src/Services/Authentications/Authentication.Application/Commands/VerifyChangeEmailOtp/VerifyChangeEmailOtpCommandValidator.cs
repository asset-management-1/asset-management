namespace Authentication.Application.Commands.VerifyChangeEmailOtp;

/// <summary>
/// Validates change-email OTP verification payloads.
/// </summary>
public class VerifyChangeEmailOtpCommandValidator : AbstractValidator<VerifyChangeEmailOtpCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VerifyChangeEmailOtpCommandValidator"/> class.
    /// </summary>
    public VerifyChangeEmailOtpCommandValidator()
    {
        RuleFor(x => x.NewEmail)
            .Required()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.Otp)
            .Required()
            .Length(OTP_LENGTH);
    }
}
