namespace Authentication.Application.Commands.ChangePassword;

/// <summary>
/// Validates authenticated password-change requests.
/// </summary>
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    /// <summary>
    /// Creates validation rules for current password and new password strength.
    /// </summary>
    public ChangePasswordCommandValidator()
    {
        // Validate the current password and enforce a strong replacement password before service-level hash checks.
        RuleFor(x => x.CurrentPassword)
            .Required();

        RuleFor(x => x.NewPassword)
            .Required()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .Matches("[@$!%*?&]")
            .WithMessage(PASSWORD_COMPLEXITY_RULES)
            .Must((request, newPassword) => !string.Equals(
                request.CurrentPassword,
                newPassword,
                StringComparison.Ordinal))
            .WithMessage(NEW_PASSWORD_MUST_DIFFER_FROM_CURRENT_PASSWORD);

    }
}
