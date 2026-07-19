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
            .MaxLen(50)
            .Must(provider => provider is EXTERNAL_PROVIDER_GOOGLE or EXTERNAL_PROVIDER_FACEBOOK)
            .WithMessage(ApplicationErrorConstants.ExternalProviderErrors.INVALID_EXTERNAL_PROVIDER_MESSAGE);

        // External token can be large but must stay within a safe request boundary.
        RuleFor(x => x.ExternalToken)
            .Required()
            .MaxLen(4000);
    }
}
