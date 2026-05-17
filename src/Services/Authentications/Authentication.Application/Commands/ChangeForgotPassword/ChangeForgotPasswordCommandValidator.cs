namespace Authentication.Application.Commands.ChangeForgotPassword;

/// <summary>
/// Validates forgot-password change requests after OTP verification.
/// </summary>
public class ChangeForgotPasswordCommandValidator : AbstractValidator<ChangeForgotPasswordCommand>
{
    /// <summary>
    /// Creates validation rules for forgot-password password replacement.
    /// </summary>
    public ChangeForgotPasswordCommandValidator()
    {
        // Validate reset-session input and password strength before the handler checks cached reset state.
        RuleFor(x => x.Email)
            .Required()
            .EmailAddress();

        RuleFor(x => x.NewPassword)
            .Required()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .Matches("[@$!%*?&]")
            .WithMessage(PASSWORD_COMPLEXITY_RULES);

    }
}
