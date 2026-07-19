namespace Authentication.Application.Commands.ForgotPassword;

/// <summary>
/// Validates forgot-password OTP request payloads.
/// </summary>
public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    /// <summary>
    /// Creates validation rules for the reset email address.
    /// </summary>
    public ForgotPasswordCommandValidator()
    {
        // Reset email is normalised later, so validate shape and size at the boundary.
        RuleFor(x => x.Email)
            .Required()
            .EmailFormat()
            .MaxLen(255);
    }
}
