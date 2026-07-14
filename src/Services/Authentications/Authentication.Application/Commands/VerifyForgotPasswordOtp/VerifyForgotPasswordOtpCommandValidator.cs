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
        // Email identifies the pending reset session that owns the OTP.
        RuleFor(x => x.Email)
            .Required()
            .EmailFormat()
            .MaxLen(255);

        // OTP length is fixed so the cache lookup and verification path stay predictable.
        RuleFor(x => x.Otp)
            .Required()
            .BetweenLen(OTP_LENGTH, OTP_LENGTH);
    }
}
