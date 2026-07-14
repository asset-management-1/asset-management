namespace Authentication.Application.Commands.VerifyChangeEmailOtp;

/// <summary>
/// Validates change-email OTP verification payloads.
/// </summary>
public class VerifyChangeEmailOtpCommandValidator : AbstractValidator<VerifyChangeEmailOtpCommand>
{
    /// <summary>
    /// Creates validation rules for verifying a pending change-email OTP.
    /// </summary>
    public VerifyChangeEmailOtpCommandValidator()
    {
        // New email identifies the pending change-email OTP request.
        RuleFor(x => x.NewEmail)
            .Required()
            .EmailFormat()
            .MaxLen(255);

        // OTP length is fixed so the cache lookup and verification path stay predictable.
        RuleFor(x => x.Otp)
            .Required()
            .BetweenLen(OTP_LENGTH, OTP_LENGTH);
    }
}
