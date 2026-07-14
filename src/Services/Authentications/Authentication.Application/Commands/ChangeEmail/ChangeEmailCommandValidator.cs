namespace Authentication.Application.Commands.ChangeEmail;

/// <summary>
/// Validates change-email OTP request payloads.
/// </summary>
public class ChangeEmailCommandValidator : AbstractValidator<ChangeEmailCommand>
{
    /// <summary>
    /// Creates validation rules for the email address that will receive the change-email OTP.
    /// </summary>
    public ChangeEmailCommandValidator()
    {
        // New email must be deliverable and bounded before OTP is issued.
        RuleFor(x => x.NewEmail)
            .Required()
            .EmailFormat()
            .MaxLen(255);
    }
}
