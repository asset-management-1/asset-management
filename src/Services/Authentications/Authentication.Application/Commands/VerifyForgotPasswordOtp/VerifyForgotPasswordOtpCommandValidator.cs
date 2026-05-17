namespace Authentication.Application.Commands.VerifyForgotPasswordOtp;

/// <summary>
/// Validates forgot-password OTP verification requests.
/// </summary>
public class VerifyForgotPasswordOtpCommandValidator : AbstractValidator<VerifyForgotPasswordOtpCommand>
{
    /// <summary>
    /// Creates validation rules for email and OTP fields.
    /// </summary>
    public VerifyForgotPasswordOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .Required()
            .EmailAddress();

        RuleFor(x => x.Otp)
            .Required()
            .Length(OTP_LENGTH);
    }
}
