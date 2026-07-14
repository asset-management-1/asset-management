namespace Authentication.Application.Commands.Register;

/// <summary>
/// Validates the register command.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>
    /// Creates validation rules for the registration payload.
    /// </summary>
    public RegisterCommandValidator()
    {
        // Registration must validate the fields used later for pending cache keys, OTP delivery, and account provisioning.
        RuleFor(x => x.UserName)
            .Required()
            .MaxLen(255);

        RuleFor(x => x.PartyType)
            .ValidEnum();

        RuleFor(x => x.Password)
            .Required()
            .MinLen(PASSWORD_MINIMUM_LENGTH, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES)
            .MatchesRegex(PASSWORD_UPPERCASE_PATTERN, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES)
            .MatchesRegex(PASSWORD_LOWERCASE_PATTERN, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES)
            .MatchesRegex(PASSWORD_NUMBER_PATTERN, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES)
            .MatchesRegex(PASSWORD_SPECIAL_CHARACTER_PATTERN, ApplicationErrorConstants.ValidationErrors.PASSWORD_COMPLEXITY_RULES);

        RuleFor(x => x.Email)
            .Required()
            .EmailFormat()
            .MaxLen(255);

        RuleFor(x => x.PhoneNumber)
            .Required()
            .MaxLen(50);

        RuleFor(x => x.FullName)
            .Required()
            .MaxLen(255);
    }
}
