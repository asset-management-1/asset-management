namespace Authentication.Application.Commands.ExternalLogin;

/// <summary>
/// Validates external-login requests before handler execution.
/// </summary>
public class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalLoginCommandValidator"/> class.
    /// </summary>
    public ExternalLoginCommandValidator()
    {
        RuleFor(x => x.Provider)
            .Required()
            .MaxLen(50);

        RuleFor(x => x.ExternalToken)
            .Required()
            .MaxLen(4000);
    }
}
