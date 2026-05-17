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
            .IsInEnum();

        RuleFor(x => x.Password)
            .Required()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .Matches("[@$!%*?&]")
            .WithMessage(PASSWORD_COMPLEXITY_RULES);

        RuleFor(x => x.Email)
            .Required()
            .EmailAddress()
            .MaxLen(255);

        RuleFor(x => x.PhoneNumber)
            .Required()
            .MaxLen(50);

        RuleFor(x => x.FullName)
            .Required()
            .MaxLen(255);
    }
}
