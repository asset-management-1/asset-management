namespace Authentication.Application.Commands.ExternalLogin;

/// <summary>
/// Validates external-login requests before handler execution.
/// </summary>
public class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    /// <summary>
    /// Creates validation rules for external provider login payloads.
    /// </summary>
    public ExternalLoginCommandValidator()
    {
        // Provider is matched by code, so keep it present and bounded before token validation.
        RuleFor(x => x.Provider)
            .Required()
            .MaxLen(50);

        // External token can be large but must stay within a safe request boundary.
        RuleFor(x => x.ExternalToken)
            .Required()
            .MaxLen(4000);
    }
}
