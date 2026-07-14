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
        // Email identifies the pending registration cache entry that owns the OTP.
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
