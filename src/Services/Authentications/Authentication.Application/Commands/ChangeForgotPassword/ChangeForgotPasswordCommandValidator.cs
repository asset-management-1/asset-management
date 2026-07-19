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
        RuleFor(x => x.PasswordResetToken)
            .Required();

        RuleFor(x => x.NewPassword)
            .Required()
            .MinLen(PASSWORD_MINIMUM_LENGTH, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES)
            .MatchesRegex(PASSWORD_UPPERCASE_PATTERN, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES)
            .MatchesRegex(PASSWORD_LOWERCASE_PATTERN, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES)
            .MatchesRegex(PASSWORD_NUMBER_PATTERN, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES)
            .MatchesRegex(PASSWORD_SPECIAL_CHARACTER_PATTERN, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES);
    }
}
