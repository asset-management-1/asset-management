namespace Authentication.Application.Commands.Login;

/// <summary>
/// Validates the login command payload.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// Creates validation rules for login credentials before lockout checks run.
    /// </summary>
    public LoginCommandValidator()
    {
        // Required login fields are validated here so missing payload values never count toward login lock attempts.
        RuleFor(x => x.UserName)
            .Required();

        RuleFor(x => x.Password)
            .Required();
    }
}
