namespace Authentication.Application.Commands.ChangeEmail;

/// <summary>
/// Validates change-email OTP request payloads.
/// </summary>
public class ChangeEmailCommandValidator : AbstractValidator<ChangeEmailCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeEmailCommandValidator"/> class.
    /// </summary>
    public ChangeEmailCommandValidator()
    {
        RuleFor(x => x.NewEmail)
            .Required()
            .EmailAddress()
            .MaximumLength(255);
    }
}
