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
            .MinLen(PASSWORD_MINIMUM_LENGTH, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES)
            .MatchesRegex(PASSWORD_UPPERCASE_PATTERN, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES)
            .MatchesRegex(PASSWORD_LOWERCASE_PATTERN, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES)
            .MatchesRegex(PASSWORD_NUMBER_PATTERN, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES)
            .MatchesRegex(PASSWORD_SPECIAL_CHARACTER_PATTERN, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES)
            .Must((request, newPassword) => !string.Equals(
                request.CurrentPassword,
                newPassword,
                StringComparison.Ordinal))
            .WithMessage(ApplicationErrorConstants.ValidationErrors.NEW_PASSWORD_MUST_DIFFER_FROM_CURRENT_PASSWORD);
    }
}
